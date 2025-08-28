using IPNoticeHub.Data.Entities.CopyrightRegistration;

namespace IPNoticeHub.Data.Repositories.Copyrights.Abstractions
{
    public interface ICopyrightRepository
    {
        Task<CopyrightEntity?> GetByPublicIdAsync(Guid publicId, bool asNoTracking = true);

        Task<CopyrightEntity?> GetByRegNumberAsync(string registrationNumber, bool asNoTracking = true);

        Task<bool> ExistsByRegNumberAsync(string registrationNumber);

        Task AddAsync(CopyrightEntity entity, CancellationToken cancellationToken = default);
    }
}
