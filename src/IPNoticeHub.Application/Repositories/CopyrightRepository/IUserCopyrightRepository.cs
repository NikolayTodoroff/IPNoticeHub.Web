using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Shared.Support;

namespace IPNoticeHub.Application.Repositories.CopyrightRepository
{
    public interface IUserCopyrightRepository
    {
        Task<bool> IsLinkedAsync(
            string userId, 
            int copyrightId, 
            bool includeSoftDeleted = false, 
            CancellationToken cancellationToken = default);

        Task AddOrUndeleteAsync(
            string userId, 
            int copyrightId, 
            CancellationToken  cancellationToken = default);

        Task<bool> SoftRemoveAsync(
            string userId, 
            int copyrightId, 
            CancellationToken cancellationToken = default);

        Task<PagedResult<UserCopyright>> GetUserCollectionPageAsync(
            string userId,
            CollectionSortBy sortBy,
            int page,
            int resultsPerPage,
            CancellationToken cancellationToken = default);
    }
}
