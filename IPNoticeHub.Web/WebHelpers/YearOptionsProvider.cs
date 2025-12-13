using Microsoft.AspNetCore.Mvc.Rendering;

namespace IPNoticeHub.Web.WebHelpers;
public static class YearOptionsProvider
{
    public static IEnumerable<SelectListItem> BuildYearOptions(int startYear = 1900, int? endYear = null, int? selected = null)
    {
        int last = endYear ?? DateTime.UtcNow.Year;

        yield return new SelectListItem { Text = "—", Value = "", Selected = selected is null };

        for (int y = last; y >= startYear; y--)
        {
            yield return new SelectListItem
            {
                Text = y.ToString(),
                Value = y.ToString(),
                Selected = selected == y
            };
        }
    }
}
