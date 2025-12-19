using IPNoticeHub.Application.Services.SystemClockService.Abstractions;
using IPNoticeHub.Infrastructure.Persistence.Services.SystemClockService.Implementation;

namespace IPNoticeHub.Web.Extensions
{
    public static class SystemClockExtension
    {
        public static IServiceCollection AddSystemClock(this IServiceCollection services)
        {
            services.AddScoped<ISystemClockService, SystemClockService>();
            return services;
        }
    }
}
