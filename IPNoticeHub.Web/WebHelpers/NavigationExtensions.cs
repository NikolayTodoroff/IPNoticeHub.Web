using Microsoft.AspNetCore.Mvc;

namespace IPNoticeHub.Web.WebHelpers
{
    public static class NavigationExtensions
    {
        public static IActionResult RedirectToLocalOrAction(this Controller controller,string? returnUrl,
            string action,string? controllerName = null,object? routeValues = null)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && controller.Url.IsLocalUrl(returnUrl))
            {
                return controller.LocalRedirect(returnUrl);
            }

            return controller.RedirectToAction(action, controllerName, routeValues);
        }
    }
}
