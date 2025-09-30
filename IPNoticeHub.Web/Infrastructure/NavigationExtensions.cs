using Microsoft.AspNetCore.Mvc;

namespace IPNoticeHub.Web.Infrastructure
{
    public static class NavigationExtensions
    {
        /// <summary>
        /// Redirect to returnUrl if it's a local URL; otherwise RedirectToAction(action, controller, routeValues).
        /// Produces LocalRedirectResult or RedirectToActionResult (test-friendly).
        /// </summary>
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
