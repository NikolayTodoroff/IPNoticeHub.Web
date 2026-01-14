using IPNoticeHub.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class TrademarkClassExtensions
    {
        public static string GetGoodsOnly(this TrademarkClass value)
        {
            var display = value.GetDisplayName();

            var parts = display.Split(new[] { "–", "-" }, 
                2, StringSplitOptions.TrimEntries);

            return parts.Length == 2 ? parts[1] : display;
        }

        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var attr = member?.GetCustomAttribute<DisplayAttribute>();
            return attr?.Name ?? value.ToString();
        }
    }
}
