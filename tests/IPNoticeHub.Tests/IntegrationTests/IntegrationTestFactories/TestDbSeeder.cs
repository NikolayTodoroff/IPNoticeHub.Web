using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Identity;

namespace IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories
{
    public static class TestDbSeeder
    {
        public static async Task SeedUserAsync(
            IPNoticeHubDbContext testDbContext, 
            string userId)
        {
            if (!testDbContext.Users.Any(u => u.Id == userId))
            {
                testDbContext.Users.Add(
                    new ApplicationUser { 
                        Id = userId, 
                        UserName = userId, 
                        Email = $"{userId}@test" });
                
                await testDbContext.SaveChangesAsync();
            }
        }

        public static async Task<CopyrightEntity> SeedCopyrightAsync(
        IPNoticeHubDbContext testDbContext, 
        string regNumber, 
        string title, 
        string typeOfWork = "Literary")
        {
            var copyrightEntity = new CopyrightEntity
            {
                RegistrationNumber = regNumber,
                TypeOfWork = typeOfWork,
                Title = title,
                Owner = "Owner",
            };

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();
            return copyrightEntity;
        }
    }
}
