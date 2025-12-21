namespace IPNoticeHub.Shared.Constants
{
    public static class ValidationConstants
    {
        public static class TrademarkRegistrationConstants
        {
            public const int WordmarkMaxLength = 300;
            public const int SourceIdMaxLength = 50;
            public const int RegistrationNumberMaxLength = 50;
            public const int GoodsAndServicesMaxLength = 4000;
            public const int OwnerNameMaxLength = 255;
            public const int TrademarkStatusDetailsMaxLength = 1000;
            public const int MarkImageUrlMaxLength = 2048;
            public const int FilterSearchTermMaxLength = 500;
            public const int WatchlistInitialStatusTextMaxLength = 1000;
        }

        public static class TrademarkEventConstants
        {
            public const int TrademarkEventCodeMaxLength = 10;
            public const int TrademarkEventDescriptionMaxLength = 500;
            public const int TrademarkEventTypeRawMaxLength = 2;
        }

        public static class TrademarkSearchFilterConstants
        {
            public const int TrademarkSearchTermMaxLength = 100;
            public const int TrademarkCurrentPageMinValue = 1;
            public const int TrademarkResultsPerPageMinValue = 20;
            public const int TrademarkResultsPerPageMaxValue = 100;
        }

        public static class CopyrightRegistrationConstants
        {
            public const int RegistrationNumberMaxLength = 20;
            public const int WorkTypeMaxLength = 50;
            public const int TitleMaxLength = 300;
            public const int OwnerNameMaxLength = 255;
            public const int NationOfFirstPublicationMaxLength = 100;
        }

        public static class LegalDocumentConstants
        {
            public const int LegalDocumentsTitleMaxLength = 256;
            public const int IpTitleMaxLength = 256;
            public const int RegistrationNumberMaxLength = 50;
        }

        public static class InfringementPlaceholderConstants
        {
            public const int SenderNameMaxLength = 256;
            public const int SenderAddressMaxLength = 512;
            public const int SenderEmailMaxLength = 256;

            public const int RecipientNameMaxLength = 256;
            public const int RecipientAddressMaxLength = 512;
            public const int RecipientEmailMaxLength = 256;

            public const int InfringingUrlMaxLength = 2048;
            public const int GoodFaithStatementMaxLength = 2048;
            public const int AdditionalFactsMaxLength = 2048;
            public const int NationOfFirstPublicationMaxLength = 256;
        }

        public static class DmcaDocumentConstants
        {
            public const string GoodFaithStatement = 
                "I have a good faith belief that the disputed use of the " +
                "copyrighted material is not authorized by the copyright owner, " +
                "its agent, or the law.";

            public const string BodyTemplate = "Dear {{RecipientName}},\n\nI, {{SenderName}} ({{SenderEmail}}), " +
            "submit this DMCA notice concerning the work \"{{WorkTitle}}\" (Reg. No. {{RegistrationNumber}}). " +
            "The infringing material appears at {{InfringingUrl}}." +
            "\n\n{{GoodFaithStatement}}\n\nSincerely,\n{{SenderName}}\n{{SenderAddress}}";
        }
    }
}
