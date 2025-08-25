namespace IPNoticeHub.Common
{
    public static class EntityValidationConstants
    {
        public static class TrademarkRegistrationConstants
        {
            // Maximum allowed length for the Wordmark field
            public const int WordmarkMaxLength = 300;

            // Maximum allowed length for the SourceId field
            public const int SourceIdMaxLength = 50;

            // Maximum allowed length for the Registration Number field, typically a 7-digit number
            public const int RegistrationNumberMaxLength = 50;

            // Maximum allowed length for the Goods and Services field
            public const int GoodsAndServicesMaxLength = 1000;

            // Maximum allowed length for the Owner Name field
            public const int OwnerNameMaxLength = 255;

            // Maximum allowed length for the Trademark Status Details field
            public const int TrademarkStatusDetailsMaxLength = 500;

            // Maximum allowed length for the Mark Image URL field
            public const int MarkImageUrlMaxLength = 2048;
        }

        public static class TrademarkEventConstants
        {
            // Maximum allowed length for the Trademark Event Code field
            public const int TrademarkEventCodeMaxLength = 10;

            // Maximum allowed length for the Trademark Event Description field
            public const int TrademarkEventDescriptionMaxLength = 500;

            // Maximum allowed length for the raw Trademark Event Type field
            public const int TrademarkEventTypeRawMaxLength = 2;
        }

        public static class CopyrightRegistrationConstants
        {
            // Maximum allowed length for the Registration Number field
            public const int RegistrationNumberMaxLength = 20;

            // Maximum allowed length for the Work Type field
            public const int WorkTypeMaxLength = 50;

            // Maximum allowed length for the Title field
            public const int TitleMaxLength = 300;

            // Maximum allowed length for the Owner Name field
            public const int OwnerNameMaxLength = 255;

            // Maximum allowed length for the Nation of Publication field
            public const int NationOfFirstPublicationMaxLength = 100;
        }
    }
}
