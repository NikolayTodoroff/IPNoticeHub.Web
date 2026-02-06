using IPNoticeHub.Application.DTOs.UserRegistrationDTOs;

namespace IPNoticeHub.Application.Services.UserRegistrationServices.Abstractions
{
    public interface IUserRegistrationService
    {
        Task<UserRegistrationResult> RegisterUserAsync(
            UserRegistrationRequest request, 
            CancellationToken cancellationToken = default);
    }
}
