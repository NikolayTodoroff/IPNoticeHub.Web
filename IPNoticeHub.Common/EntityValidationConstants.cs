namespace IPNoticeHub.Common
{
    public static class EntityValidationConstants
    {
        public static class TrademarkRegistrationConstants
        {
            // Maximum allowed length for the Wordmark field
            public const int WordmarkMaxLength = 200;

            // Maximum allowed length for the SourceId field
            public const int SourceIdMaxLength = 100;

            // Maximum allowed length for the Registration Number field, typically a 7-digit number
            public const int RegistrationNumberMaxLength = 7;

            // Maximum allowed length for the Goods and Services field
            public const int GoodsAndServicesMaxLength = 2000;

            // Maximum allowed length for the Owner Name field
            public const int OwnerNameMaxLength = 250;

            // Maximum allowed length for the Trademark Status Details field
            public const int TrademarkStatusDetailsMaxLength = 300;

            // Maximum allowed length for the Mark Image URL field
            public const int MarkImageUrlMaxLength = 1000;
        }

        public static class CopyrightRegistrationConstants
        {
            public const int TitleMaxLength = 200;
        }
    }
}
