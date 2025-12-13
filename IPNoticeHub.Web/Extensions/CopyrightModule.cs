using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Application.Services.CopyrightService.Implementations;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;

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
