namespace IPNoticeHub.Application.DTOs.UserRegistrationDTOs
{
    public sealed record UserRegistrationResult(
        bool Succeeded,
        IReadOnlyList<string> Errors);
}
