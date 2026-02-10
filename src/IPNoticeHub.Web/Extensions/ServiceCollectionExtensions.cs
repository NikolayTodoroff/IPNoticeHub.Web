using IPNoticeHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
        {
            // Database registration is skipped in test environment (tests configure their own database)
            if (environment.IsEnvironment("Test"))
            {
                return services;
            }

            // Database registration is skipped if it's already configured (in tests)
            if (services.Any(
                s => s.ServiceType == typeof(DbContextOptions<IPNoticeHubDbContext>) ||
                s.ServiceType == typeof(IPNoticeHubDbContext)))
            {
                return services;
            }

            var connectionString = 
                configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' missing");

            services.AddDbContext<IPNoticeHubDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddDatabaseDeveloperPageExceptionFilter();

            return services;
        }
    }
}
