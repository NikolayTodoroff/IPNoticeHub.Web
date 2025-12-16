using IPNoticeHub.Application.DTOs.UserRegistrationDTOs;
using IPNoticeHub.Application.Services.UserRegistrationServices.Abstractions;
using IPNoticeHub.Shared.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IPNoticeHub.Infrastructure.Identity
{
    public sealed class UserRegistrationService : IUserRegistrationService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<UserRegistrationService> logger;

        public UserRegistrationService(
            UserManager<ApplicationUser> userManager, 
            ILogger<UserRegistrationService> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<UserRegistrationResult> RegisterUserAsync(
            UserRegistrationRequest request, 
            CancellationToken cancellationToken = default)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                return new UserRegistrationResult(
                    false,
                    createResult.Errors.Select(e => e.Description).ToList());
            }

            var roleResult = 
                await userManager.AddToRoleAsync(user, RoleNames.User);

            if (!roleResult.Succeeded)
            {
                var errors = 
                    roleResult.Errors.Select(e => e.Description).ToList();

                logger.LogCritical(
                    "Registration created user {Email} but failed to assign role {Role}. " +
                    "Errors: {Errors}",
                    request.Email,
                    RoleNames.User,
                    string.Join(", ", errors));

                var deleteResult = await userManager.DeleteAsync(user);

                if (!deleteResult.Succeeded)
                {
                    var deleteErrors = 
                        deleteResult.Errors.Select(e => e.Description).ToList();

                    logger.LogCritical(
                        "Failed to delete orphaned user {Email} " +
                        "after role assignment failure. Errors: {Errors}",
                        request.Email,
                        string.Join(", ", deleteErrors));
                }

                return new UserRegistrationResult(false, errors);
            }

            return new UserRegistrationResult(true, Array.Empty<string>());
        }
    }
}
