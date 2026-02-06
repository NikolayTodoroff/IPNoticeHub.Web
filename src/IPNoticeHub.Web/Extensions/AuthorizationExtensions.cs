using System.Security.Claims;

namespace IPNoticeHub.Web.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("HasUserId",
                    p => p.RequireClaim(ClaimTypes.NameIdentifier));
            });

            return services;
        }
    }
}
