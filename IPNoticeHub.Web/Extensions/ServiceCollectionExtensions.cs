using IPNoticeHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
        {
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
