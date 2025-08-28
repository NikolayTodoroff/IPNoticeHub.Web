using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.CopyrightRegistration;

namespace IPNoticeHub.Data.Repositories.Copyrights.Abstractions
{
    public interface IUserCopyrightRepository
    {
        Task<bool> IsLinkedAsync(string userId, int copyrightId, bool includeSoftDeleted = false);

        Task AddOrUndeleteAsync(string userId, int copyrightId, CancellationToken  cancellationToken = default);

        Task<bool> SoftRemoveAsync(string userId, int copyrightId, CancellationToken cancellationToken = default);

        IQueryable<CopyrightEntity> QueryUserCollection(string userId);

        IQueryable<UserCopyright> QueryUserLinks(string userId);
    }
}
