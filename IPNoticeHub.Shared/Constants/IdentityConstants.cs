namespace IPNoticeHub.Shared.Constants
{
    public static class IdentityConstants
    {
        public static class AdminAccountCredentials
        {
            public const string AdminEmailAddress = "admin@ipnoticehub.com";
            public const string AdminEmailPassword = "Admin!234";
        }

        public static class AuthRedirectPaths
        {
            public const string LoginPath = "/Identity/Account/Login";
            public const string AccessDeniedPath = "/Identity/Account/AccessDenied";
        }
    }
}
