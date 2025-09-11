using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data.Entities.CopyrightRegistration;
using IPNoticeHub.Data.Repositories.Copyrights.Abstractions;

namespace IPNoticeHub.Data.Repositories.Copyrights.Implementations
{
    public sealed class CopyrightRepository : ICopyrightRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public CopyrightRepository(IPNoticeHubDbContext context)
        {
            this.dbContext = context;
        }

        public async Task AddAsync(CopyrightEntity entity, CancellationToken cancellationToken = default)
        {
            entity.RegistrationNumber = NormalizeReg(entity.RegistrationNumber);

            await dbContext.Set<CopyrightEntity>().AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task UpdateAsync(CopyrightEntity entity, CancellationToken cancellationToken = default)
        {
            dbContext.CopyrightRegistrations.Update(entity);
            return dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> ExistsByRegNumberAsync(string registrationNumber, CancellationToken cancellationToken = default)
        {
            var regNumber = NormalizeReg(registrationNumber);
            return dbContext.Set<CopyrightEntity>()
                     .AnyAsync(c => c.RegistrationNumber == regNumber, cancellationToken);
        }

        public Task<CopyrightEntity?> GetByPublicIdAsync(Guid publicId, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Set<CopyrightEntity>().
                Where(c => c.PublicId == publicId);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            } 

            return query.SingleOrDefaultAsync(cancellationToken);
        }

        public Task<CopyrightEntity?> GetByRegNumberAsync(string registrationNumber, bool asNoTracking = true,CancellationToken cancellationToken = default)
        {
            var regNumber = NormalizeReg(registrationNumber);

            var query = dbContext.Set<CopyrightEntity>().
                Where(c => c.RegistrationNumber == regNumber);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            } 

            return query.SingleOrDefaultAsync(cancellationToken);
        }

        private static string NormalizeReg(string input) => (input ?? string.Empty).Trim().ToUpperInvariant();
    }
}
