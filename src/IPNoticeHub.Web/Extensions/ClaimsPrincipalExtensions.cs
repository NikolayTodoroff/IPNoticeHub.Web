using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace IPNoticeHub.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TryGetUserId(this ClaimsPrincipal user, [NotNullWhen(true)] out string? userId)
        {
            userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(userId);
        }
    }
}
