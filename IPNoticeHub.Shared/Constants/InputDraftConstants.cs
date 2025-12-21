namespace IPNoticeHub.Shared.Constants
{
    public static class InputDraftConstants
    {
        public static class UserInputDraftOptions
        {
            public static readonly TimeSpan InputTtl = TimeSpan.FromMinutes(30);

            public const string CopyrightCadKeySpace = "copyright-cad-input-draft";

            public const string CopyrightDmcaKeySpace = "copyright-dmca-input-draft";

            public const string TrademarkCadKeySpace = "trademark-cad-input-draft";
        }
    }
}
