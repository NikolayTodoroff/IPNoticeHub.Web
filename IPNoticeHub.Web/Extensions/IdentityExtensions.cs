using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace IPNoticeHub.Web.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<IPNoticeHubDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            return services;
        }
    }
}
