using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.CopyrightRegistration;
using IPNoticeHub.Data.Repositories.Copyrights.Abstractions;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Services.Copyrights.Implementations
{
    public sealed class CopyrightService : ICopyrightService
    {
        private readonly ICopyrightRepository copyrightRepository;
        private readonly IUserCopyrightRepository userCopyrightRepository;

        public CopyrightService(ICopyrightRepository copyrights,IUserCopyrightRepository userCopyrights)
        {
            this.copyrightRepository = copyrights;
            this.userCopyrightRepository = userCopyrights;
        } 

        public async Task<Guid> CreateAsync(string userId, CopyrightCreateDto dto, CancellationToken cancellationToken = default)
        {
            var existingEntity = await copyrightRepository.
                GetByRegNumberAsync(dto.RegistrationNumber, asNoTracking: false, cancellationToken: cancellationToken);

            CopyrightEntity newEntity;

            if (existingEntity is null)
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
                newEntity = existingEntity;
            }

            await userCopyrightRepository.AddOrUndeleteAsync(userId, newEntity.Id, cancellationToken);

            return newEntity.PublicId;
        }

        public async Task<bool> EditAsync(string userId, Guid publicId, CopyrightEditDTO dto, CancellationToken cancellationToken = default)
        {
            var copyrightEntity = await copyrightRepository.
                GetByPublicIdAsync(publicId, asNoTracking: false, cancellationToken: cancellationToken);

            if (copyrightEntity is null)
            {
                return false;
            }

            var linkedInCollection = await userCopyrightRepository.
                IsLinkedAsync(userId, copyrightEntity.Id, includeSoftDeleted: false, cancellationToken: cancellationToken);

            if (!linkedInCollection)
            {
                return false;
            }

            // Preventing duplicate registration numbers if changed
            if (!string.Equals(copyrightEntity.RegistrationNumber, dto.RegistrationNumber, StringComparison.OrdinalIgnoreCase))
            {
                var registrationExists = await copyrightRepository.ExistsByRegNumberAsync(dto.RegistrationNumber, cancellationToken);

                if (registrationExists)
                {
                    return false;
                }

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

        public async Task<CopyrightDetailsDTO?> GetDetailsAsync(string userId, Guid publicId, CancellationToken cancellationToken = default)
        {
            CopyrightEntity? entity = await copyrightRepository.GetByPublicIdAsync(publicId, asNoTracking: true,cancellationToken: cancellationToken);

            if (entity is null) return null;

            bool linked = await userCopyrightRepository.IsLinkedAsync(userId, entity.Id, includeSoftDeleted: false,cancellationToken: cancellationToken);
            if (!linked) return null;

            return new CopyrightDetailsDTO()
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

        public async Task<PagedResult<CopyrightListItemDTO>> GetUserCollectionAsync(string userId, CollectionSortBy sortBy, int page, int resultsPerPage, CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(page, resultsPerPage);

            IQueryable<UserCopyright>? links = userCopyrightRepository.QueryUserLinks(userId);

            if (sortBy == CollectionSortBy.DateAddedAsc)
            {
                links = links.OrderBy(l => l.DateAdded).ThenBy(l => l.CopyrightRegistrationId);
            }

            else if (sortBy == CollectionSortBy.TitleAsc)
            {
                links = links.OrderBy(l => l.CopyrightRegistration.Title).ThenBy(l => l.CopyrightRegistrationId);
            }

            else if (sortBy == CollectionSortBy.TitleDesc)
            {
                links = links.OrderByDescending(l => l.CopyrightRegistration.Title).ThenBy(l => l.CopyrightRegistrationId);
            }

            else
            {
                links = links.OrderByDescending(l => l.DateAdded).ThenBy(l => l.CopyrightRegistrationId);
            }

            int resultsCount = await links.AsNoTracking().CountAsync(cancellationToken);

            List<CopyrightListItemDTO>? results = await links.
                Skip((normalizedPage - 1) * normalizedPageSize).
                Take(normalizedPageSize).
                Select(l=> new CopyrightListItemDTO()
                {
                    PublicId = l.CopyrightRegistration.PublicId,
                    RegistrationNumber = l.CopyrightRegistration.RegistrationNumber,
                    TypeOfWork = l.CopyrightRegistration.TypeOfWork,
                    Title = l.CopyrightRegistration.Title,
                    YearOfCreation = l.CopyrightRegistration.YearOfCreation,
                    DateOfPublication = l.CopyrightRegistration.DateOfPublication,
                    Owner = l.CopyrightRegistration.Owner,
                    DateAdded = l.DateAdded
                }).
                ToListAsync(cancellationToken);

            return new PagedResult<CopyrightListItemDTO>()
            {
                Results = results,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }

        public async Task<bool> RemoveAsync(string userId, Guid publicId, CancellationToken cancellationToken = default)
        {
            CopyrightEntity? entity = await copyrightRepository.GetByPublicIdAsync(publicId, asNoTracking : true, cancellationToken: cancellationToken);

            if (entity is null) return false;

            return await userCopyrightRepository.SoftRemoveAsync(userId,entity.Id, cancellationToken);
        }

        private static string NormalizeWorkType(CopyrightWorkType workType, string? other = null)
        {
            return workType == CopyrightWorkType.Other ? (other ?? string.Empty).Trim() : workType.ToString();
        }
    }
}
