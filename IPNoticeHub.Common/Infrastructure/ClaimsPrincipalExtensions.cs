using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace IPNoticeHub.Common.Extensions
{
    /// <summary>
    /// Attempts to retrieve the user ID from the claims of the current user.
    /// Returns True if the user ID was successfully retrieved; otherwise, returns False.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        public static bool TryGetUserId(this ClaimsPrincipal user, [NotNullWhen(true)] out string? userId)
        {
            userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(userId);
        }
    }
}
