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
                EmailConfirmed = false
            };

            var createUser = await userManager.CreateAsync(
                user, 
                request.Password);

            if (!createUser.Succeeded)
            {
                return UserRegistrationResult.Failure(
                    createUser.Errors.Select(e => e.Description));
            }

            IdentityResult roleResult;

            try
            {
                 roleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
            }

            catch (InvalidOperationException ex)
            {

                await DeleteOrphanedUserAsync(
                    user,
                    request.Email,
                    "Failed to assign role.");

                logger.LogCritical(
                    ex,
                    "Registration created user {Email} but role {Role} " +
                    "assignment failed due to an exception. " +
                    "This indicates a system or configuration error.",
                    request.Email,
                    RoleNames.User);

                return UserRegistrationResult.Failure(RoleAssignmentSystemError);
            }

            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description).ToList();

                await DeleteOrphanedUserAsync(
                    user,
                    request.Email,
                    "Role assignment failure");

                logger.LogCritical(
                    "Registration created user {Email} " +
                    "but failed to assign role {Role}. Identity errors: {Errors}",
                    request.Email,
                    RoleNames.User,
                    string.Join(", ", errors));

                return UserRegistrationResult.Failure(errors);
            }

            var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

            return UserRegistrationResult.Success(user.Id, emailToken);
        }

        private async Task DeleteOrphanedUserAsync(
            ApplicationUser user,
            string email,
            string contextMessage)
        {
            var deleteResult = await userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                var deleteErrors = 
                    deleteResult.Errors.Select(e => e.Description);

                logger.LogCritical(
                    "Failed to delete orphaned user {Email} after {Context}. " +
                    "Errors: {Errors}",
                    email,
                    contextMessage,
                    string.Join(", ", deleteErrors));
            }
        }

        private static IReadOnlyCollection<string> MapIdentityErrors(IEnumerable<IdentityError> errors)
        {
            var messages = errors
                .Select(e => e.Description?.Trim())
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct()
                .ToList();

            return messages.Count == 0
                ? new[] { "Registration failed. Please review your details and try again." }
                : messages!;
        }

        private static readonly IReadOnlyCollection<string> RoleAssignmentSystemError = 
            new[] { "We couldn’t complete registration due to a system configuration issue. " +
                "Please try again later." };
    }
}
