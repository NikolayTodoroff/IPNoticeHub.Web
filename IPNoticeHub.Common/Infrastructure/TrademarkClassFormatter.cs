using System.ComponentModel.DataAnnotations;
using System.Reflection;
using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Common.Infrastructure
{
    /// <summary>
    /// Provides utility methods for formatting trademark classes.
    /// </summary>
    public static class TrademarkClassFormatter
    {
        /// <summary>
        /// Retrieves the display name of a trademark class enum value.
        /// Returns the display name of the enum value, or its string representation if no display name is found.
        /// /// </summary>
        private static string GetTrademarkDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .Name ?? enumValue.ToString();
        }

        /// <summary>
        /// Formats a collection of trademark class integers into a comma-separated string of their display names.
        /// Returns a comma-separated string of trademark class display names, or an empty string if the collection is null or empty.
        /// /// </summary>
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
