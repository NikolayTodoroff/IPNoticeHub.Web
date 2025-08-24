using IPNoticeHub.Data.EnumConstants;

namespace IPNoticeHub.Common.AdditionalConfigurations
{
    public static class TrademarkEventTypeMapper
    {
        public static TrademarkEventType MapFromRaw(string? rawCode)
        {
            if (string.IsNullOrWhiteSpace(rawCode))
                return TrademarkEventType.Other;

            return rawCode.ToUpperInvariant() switch
            {
                "O" => TrademarkEventType.Office,
                "I" => TrademarkEventType.Incoming,
                "E" => TrademarkEventType.Electronic,
                "A" => TrademarkEventType.Assignment,
                "R" => TrademarkEventType.Renewal,
                "P" => TrademarkEventType.Publication,
                "D" => TrademarkEventType.Deadline,
                _ => TrademarkEventType.Other
            };
        }
    }
}
