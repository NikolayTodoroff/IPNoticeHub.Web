using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Common.Infrastructure
{
    /// <summary>
    /// Provides a mapping utility to convert raw event type codes into their corresponding enumeration values. 
    /// This helps standardize and interpret raw event type codes received from external sources.
    /// </summary>
    public static class TrademarkEventTypeMapper
    {
        public static TrademarkEventType MapFromRawEventType(string? rawCode)
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
