namespace IPNoticeHub.Application.DTOs.UserRegistrationDTOs
{
    public sealed record UserRegistrationRequest(
        string Email,
        string Password);
}
