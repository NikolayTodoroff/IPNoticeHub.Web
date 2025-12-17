using IPNoticeHub.Application.DTOs.UserRegistrationDTOs;
using IPNoticeHub.Application.Services.UserRegistrationServices.Abstractions;
using IPNoticeHub.Shared.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
                EmailConfirmed = false
            };

            var createUser = await userManager.CreateAsync(user, request.Password);

            if (!createUser.Succeeded)
            {
                return UserRegistrationResult.Failure(
                    createUser.Errors.Select(e => e.Description));
            }   

            var role = await userManager.AddToRoleAsync(user, RoleNames.User);

            if (!role.Succeeded)
            {
                var errors = role.Errors.Select(e => e.Description).ToList();

                var delete = await userManager.DeleteAsync(user);

                if (!delete.Succeeded)
                {
                    var deleteErrors = 
                        delete.Errors.Select(e => e.Description);

                    logger.LogCritical(
                        "Failed to delete orphaned user {Email} after role assignment failure. " +
                        "Errors: {Errors}",
                        request.Email, string.Join(", ", deleteErrors));
                }

                logger.LogCritical(
                    "Registration created user {Email} but failed to assign role {Role}. " +
                    "Errors: {Errors}",
                    request.Email, RoleNames.User, string.Join(", ", errors));

                return UserRegistrationResult.Failure(errors);
            }

            var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

            return UserRegistrationResult.Success(user.Id, emailToken);
        }
    }

}
