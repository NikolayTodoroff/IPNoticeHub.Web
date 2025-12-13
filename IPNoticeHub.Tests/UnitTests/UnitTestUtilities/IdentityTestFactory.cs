using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace IPNoticeHub.Tests.UnitTests.UnitTestUtilities
{
    public class IdentityTestFactory
    {
        public static ServiceProvider BuildIdentityServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddDbContext<IPNoticeHubDbContext>(options =>
                options.UseInMemoryDatabase("IdentityTestDb"));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<IPNoticeHubDbContext>()
            .AddDefaultTokenProviders();

            return services.BuildServiceProvider();
        }
    }
}
