using IPNoticeHub.Data.Repositories.Copyrights.Abstractions;
using IPNoticeHub.Data.Repositories.Copyrights.Implementations;
using IPNoticeHub.Application.Copyrights.Abstractions;
using IPNoticeHub.Application.Copyrights.Implementations;


namespace IPNoticeHub.Web.Extensions
{
    public static class CopyrightModule
    {
        public static IServiceCollection AddCopyrightModule(this IServiceCollection services)
        {
            services.AddScoped<ICopyrightRepository, CopyrightRepository>();
            services.AddScoped<IUserCopyrightRepository, UserCopyrightRepository>();

            services.AddScoped<ICopyrightService, CopyrightService>();

            return services;
        }
    }
}
