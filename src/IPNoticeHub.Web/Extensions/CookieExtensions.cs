namespace IPNoticeHub.Web.Extensions
{
    public static class CookieExtensions
    {
        public static IServiceCollection AddCookieConfiguration(this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            return services;
        }
    }
}
