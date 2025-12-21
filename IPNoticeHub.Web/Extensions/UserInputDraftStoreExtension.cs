using IPNoticeHub.Application.Services.DraftServices.Abstractions;
using IPNoticeHub.Application.Services.UserRegistrationServices.Abstractions;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence.Services.DraftServices.Implementations;

namespace IPNoticeHub.Web.Extensions
{
    public static class UserInputDraftStoreExtension
    {
        public static IServiceCollection AddUserInputDraftStore(this IServiceCollection services)
        {
            services.AddSingleton<IUserInputDraftStore, UserInputDraftStore>();
            return services;
        }
    }
}
