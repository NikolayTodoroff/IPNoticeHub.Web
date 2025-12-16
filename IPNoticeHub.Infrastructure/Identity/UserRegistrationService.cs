using IPNoticeHub.Application.Services.UserRegistrationServices.Abstractions;
using IPNoticeHub.Application.DTOs.UserRegistrationDTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;


namespace IPNoticeHub.Infrastructure.Identity
{
    public sealed class UserRegistrationService : IUserRegistrationService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<UserRegistrationService> logger;

        public UserRegistrationService(UserManager<ApplicationUser> userManager, ILogger<UserRegistrationService> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public Task<UserRegistrationResult> RegisterUserAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
