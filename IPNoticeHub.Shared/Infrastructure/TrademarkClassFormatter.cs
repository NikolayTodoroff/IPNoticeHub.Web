using System.ComponentModel.DataAnnotations;
using System.Reflection;
using IPNoticeHub.Shared.Enums;

namespace IPNoticeHub.Shared.Infrastructure
{
    public static class TrademarkClassFormatter
    {
        private static string GetTrademarkDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .Name ?? enumValue.ToString();
        }

        public static string FormatTrademarkClasses(IEnumerable<int> classes)
        {
            if (classes == null || !classes.Any())
                return string.Empty;

            return string.Join(", ", classes
                .OrderBy(c => c)
                .Select(c => GetTrademarkDisplayName((TrademarkClass)c)));
        }
    }
}
