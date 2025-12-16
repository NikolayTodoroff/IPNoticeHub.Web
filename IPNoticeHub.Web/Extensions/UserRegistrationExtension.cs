using IPNoticeHub.Application.Services.UserRegistrationServices.Abstractions;
using IPNoticeHub.Infrastructure.Identity;

namespace IPNoticeHub.Web.Extensions
{
    public static class UserRegistrationExtension
    {
        public static IServiceCollection AddUserRegistration(this IServiceCollection services)
        {
            services.AddSingleton<IUserRegistrationService, UserRegistrationService>();

            return services;
        }
    }
}
