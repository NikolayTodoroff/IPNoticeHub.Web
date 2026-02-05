using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Shared.Support;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;

namespace IPNoticeHub.Application.Services.CopyrightService.Implementations
{
    public sealed class CopyrightService : ICopyrightService
    {
        private readonly ICopyrightRepository copyrightRepository;
        private readonly IUserCopyrightRepository userCopyrightRepository;

        public CopyrightService(
            ICopyrightRepository copyrights,
            IUserCopyrightRepository userCopyrights)
        {
            this.copyrightRepository = copyrights;
            this.userCopyrightRepository = userCopyrights;
        } 

        public async Task<Guid> CreateAsync(
            string userId, 
            CopyrightCreateDto dto, 
            CancellationToken cancellationToken = default)
        {
            var entity = await copyrightRepository.
                GetByRegNumberAsync(
                dto.RegistrationNumber, 
                asNoTracking: false, 
                cancellationToken: cancellationToken);

            CopyrightEntity newEntity;

            if (entity is null)
            {
                newEntity = new CopyrightEntity()
                {
                    RegistrationNumber = dto.RegistrationNumber,
                    TypeOfWork = NormalizeWorkType(dto.WorkType, dto.OtherWorkType),
                    Title = dto.Title,
                    YearOfCreation = dto.YearOfCreation,
                    DateOfPublication = dto.DateOfPublication,
                    Owner = dto.Owner,
                    NationOfFirstPublication = dto.NationOfFirstPublication
                };

                await copyrightRepository.AddAsync(newEntity,cancellationToken);
            }

            else
            {
                newEntity = entity;
            }

            await userCopyrightRepository.
                AddOrUndeleteAsync(userId, newEntity.Id, cancellationToken);

            return newEntity.PublicId;
        }

        public async Task<bool> EditAsync(
            string userId, 
            Guid publicId, 
            CopyrightEditDto dto, 
            CancellationToken cancellationToken = default)
        {
            var copyrightEntity = await copyrightRepository.
                GetByPublicIdAsync(
                publicId, 
                asNoTracking: false, 
                cancellationToken: cancellationToken);

            if (copyrightEntity is null) return false;

            var linkedInCollection = await userCopyrightRepository.
                IsLinkedAsync(
                userId, 
                copyrightEntity.Id, 
                includeSoftDeleted: false, 
                cancellationToken: cancellationToken);

            if (!linkedInCollection) return false;

            if (!string.Equals(
                copyrightEntity.RegistrationNumber, 
                dto.RegistrationNumber, 
                StringComparison.OrdinalIgnoreCase))
            {
                bool registrationExists = await copyrightRepository.
                    ExistsByRegNumberAsync(dto.RegistrationNumber, cancellationToken);

                if (registrationExists) return false;

                copyrightEntity.RegistrationNumber = dto.RegistrationNumber;
            }

            copyrightEntity.TypeOfWork = NormalizeWorkType(dto.WorkType, dto.OtherWorkType);
            copyrightEntity.Title = dto.Title;
            copyrightEntity.YearOfCreation = dto.YearOfCreation;
            copyrightEntity.DateOfPublication = dto.DateOfPublication;
            copyrightEntity.Owner = dto.Owner;
            copyrightEntity.NationOfFirstPublication = dto.NationOfFirstPublication;

            await copyrightRepository.UpdateAsync(copyrightEntity, cancellationToken);
            return true;
        }

        public async Task<CopyrightDetailsDto?>GetDetailsAsync(
            string userId, 
            Guid publicId, 
            CancellationToken cancellationToken = default)
        {
            CopyrightEntity? entity = await copyrightRepository.
                GetByPublicIdAsync(
                publicId, 
                asNoTracking: true,
                cancellationToken: cancellationToken);

            if (entity is null) return null;

            bool linked = await userCopyrightRepository.
                IsLinkedAsync(
                userId, 
                entity.Id, 
                includeSoftDeleted: false,
                cancellationToken: cancellationToken);

            if (!linked) return null;

            return new CopyrightDetailsDto()
            {
                PublicId = entity.PublicId,
                RegistrationNumber = entity.RegistrationNumber,
                TypeOfWork = entity.TypeOfWork,
                Title = entity.Title,
                YearOfCreation = entity.YearOfCreation,
                DateOfPublication = entity.DateOfPublication,
                Owner = entity.Owner,
                NationOfFirstPublication = entity.NationOfFirstPublication
            };
        }

        public async Task<PagedResult<CopyrightSingleItemDto>> GetUserCollectionAsync(
            string userId,
            CollectionSortBy sortBy,
            int page,
            int resultsPerPage,
            CancellationToken cancellationToken = default)
        {
            var pageResult = 
                await userCopyrightRepository.GetUserCollectionPageAsync(
                userId,
                sortBy,
                page,
                resultsPerPage,
                cancellationToken);

            var mappedResults = pageResult.Results
                .Select(uc => new CopyrightSingleItemDto
                {
                    Id = uc.CopyrightEntityId,
                    PublicId = uc.CopyrightEntity.PublicId,
                    RegistrationNumber = uc.CopyrightEntity.RegistrationNumber,
                    TypeOfWork = uc.CopyrightEntity.TypeOfWork,
                    Title = uc.CopyrightEntity.Title,
                    YearOfCreation = uc.CopyrightEntity.YearOfCreation,
                    DateOfPublication = uc.CopyrightEntity.DateOfPublication,
                    Owner = uc.CopyrightEntity.Owner,
                    DateAdded = uc.DateAdded
                }).
                ToList();

            return new PagedResult<CopyrightSingleItemDto>
            {
                Results = mappedResults,
                ResultsCount = pageResult.ResultsCount,
                CurrentPage = pageResult.CurrentPage,
                ResultsCountPerPage = pageResult.ResultsCountPerPage
            };
        }


        public async Task<bool>RemoveAsync(
            string userId, 
            Guid publicId, 
            CancellationToken cancellationToken = default)
        {
            CopyrightEntity? entity = await copyrightRepository.
                GetByPublicIdAsync(
                publicId, 
                asNoTracking : true, 
                cancellationToken: cancellationToken);

            if (entity is null) return false;

            return await userCopyrightRepository.
                SoftRemoveAsync(userId,entity.Id, cancellationToken);
        }

        private static string NormalizeWorkType(CopyrightWorkType workType, string? other = null)
        {
            return workType == CopyrightWorkType.Other ? 
                (other ?? string.Empty).Trim() : workType.ToString();
        }
    }
}
