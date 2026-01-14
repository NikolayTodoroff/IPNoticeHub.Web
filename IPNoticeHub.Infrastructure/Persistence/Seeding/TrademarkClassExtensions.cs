using IPNoticeHub.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class TrademarkClassExtensions
    {
        public static string GetGoodsOnly(this TrademarkClass value)
        {
            var displayName = value.GetDisplayName();

            var nameParts = displayName.Split(new[] { "–", "-" }, 
                2, StringSplitOptions.TrimEntries);

            return nameParts.Length == 2 ? nameParts[1] : displayName;
        }

        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var attribute = member?.GetCustomAttribute<DisplayAttribute>();

            return attribute?.Name ?? value.ToString();
        }
    }
}
