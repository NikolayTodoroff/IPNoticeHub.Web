using IPNoticeHub.Data.Entities.CopyrightRegistration;

namespace IPNoticeHub.Data.Repositories.Copyrights.Abstractions
{
    public interface ICopyrightRepository
    {
        Task AddAsync(CopyrightEntity entity, CancellationToken cancellationToken = default);

        Task UpdateAsync(CopyrightEntity entity, CancellationToken cancellationToken = default);

        Task<CopyrightEntity?> GetByPublicIdAsync(Guid publicId, bool asNoTracking = true, CancellationToken cancellationToken = default);

        Task<CopyrightEntity?> GetByRegNumberAsync(string registrationNumber, bool asNoTracking = true,CancellationToken cancellationToken=default);

        Task<bool> ExistsByRegNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
    }
}
