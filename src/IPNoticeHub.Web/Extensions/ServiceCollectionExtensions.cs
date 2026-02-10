using IPNoticeHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace IPNoticeHub.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
        {
            // Skip database registration in test environment - tests will configure their own database
            if (environment.IsEnvironment("Test"))
            {
                return services;
            }

            // Skip database registration if already configured (e.g., in tests)
            if (services.Any(s => s.ServiceType == typeof(DbContextOptions<IPNoticeHubDbContext>) ||
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
