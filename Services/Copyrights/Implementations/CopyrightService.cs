using IPNoticeHub.Common.AdditionalConfigurations;
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
        private readonly ICopyrightRepository copyrights;
        private readonly IUserCopyrightRepository userCopyrights;

        public CopyrightService(ICopyrightRepository copyrights,IUserCopyrightRepository userCopyrights)
        {
            this.copyrights = copyrights;
            this.userCopyrights = userCopyrights;
        } 

        public async Task<Guid> CreateAsync(string userId, CopyrightCreateDTO dto, CancellationToken cancellationToken = default)
        {
            var existingEntity = await copyrights.GetByRegNumberAsync(dto.RegistrationNumber, asNoTracking: false);

            CopyrightEntity newEntity;

            if (existingEntity is null)
            {
                newEntity = new CopyrightEntity()
                {
                    RegistrationNumber = dto.RegistrationNumber,
                    TypeOfWork = dto.TypeOfWork,
                    Title = dto.Title,
                    YearOfCreation = dto.YearOfCreation,
                    DateOfPublication = dto.DateOfPublication,
                    Owner = dto.Owner,
                    NationOfFirstPublication = dto.NationOfFirstPublication
                };

                await copyrights.AddAsync(newEntity,cancellationToken);
            }

            else
            {
                newEntity = existingEntity;
            }

            await userCopyrights.AddOrUndeleteAsync(userId, newEntity.Id, cancellationToken);

            return newEntity.PublicId;
        }

        public async Task<CopyrightDetailsDTO?> GetDetailsAsync(string userId, Guid publicId, CancellationToken cancellationToken = default)
        {
            CopyrightEntity? entity = await copyrights.GetByPublicIdAsync(publicId, asNoTracking: true);

            if (entity is null) return null;

            bool linked = await userCopyrights.IsLinkedAsync(userId, entity.Id, includeSoftDeleted: false);
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

            IQueryable<UserCopyright>? links = userCopyrights.QueryUserLinks(userId);

            if (sortBy == CollectionSortBy.DateAddedAsc)
            {
                links = links.OrderBy(l => l.DateAdded);
            }

            else if (sortBy == CollectionSortBy.TitleAsc)
            {
                links = links.OrderBy(l => l.CopyrightRegistration.Title);
            }

            else if (sortBy == CollectionSortBy.TitleDesc)
            {
                links = links.OrderByDescending(l => l.CopyrightRegistration.Title);
            }

            else
            {
                links = links.OrderByDescending(l => l.DateAdded);
            }

            int resultsCount = await links.CountAsync(cancellationToken);

            List<CopyrightListItemDTO>? results = await links.
                Skip((normalizedPage - 1) * normalizedPageSize).
                Take(normalizedPageSize).
                Select(l=> new CopyrightListItemDTO()
                {
                    PublicId = l.CopyrightRegistration.PublicId,
                    RegistrationNumber = l.CopyrightRegistration.RegistrationNumber,
                    Title = l.CopyrightRegistration.Title,
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
            CopyrightEntity? entity = await copyrights.GetByPublicIdAsync(publicId, asNoTracking : true);

            if (entity == null) return false;

            return await userCopyrights.SoftRemoveAsync(userId,entity.Id, cancellationToken);
        }
    }
}
