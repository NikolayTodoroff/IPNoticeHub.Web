namespace IPNoticeHub.Application.DTOs.UserRegistrationDTOs
{
    public sealed record UserRegistrationResult(
    bool Succeeded,
    IReadOnlyList<string> Errors,
    string? UserId,
    string? EmailConfirmationToken)
    {
        public static UserRegistrationResult Success(string userId, string emailToken)
        {
            return new UserRegistrationResult (
                true, 
                Array.Empty<string>(), 
                userId, 
                emailToken);
        }
        
        public static UserRegistrationResult Failure(IEnumerable<string> errors)
        {
            return new(
                false, 
                errors.ToList(), 
                null, 
                null);
        }      
    }
}
