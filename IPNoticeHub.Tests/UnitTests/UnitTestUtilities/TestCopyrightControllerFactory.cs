using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.TestUtilities
{
    public static class TestCopyrightControllerFactory
    {
        /// <summary>
        /// Creates a CopyrightsController with optional user, TempData, and UrlHelper.
        /// </summary>
        public static CopyrightsController CreateController(
            ICopyrightService? service = null,
            string? userId = "u1",
            bool includeTempData = true,
            bool includeUrlHelper = true)
        {
            var httpContext = new DefaultHttpContext();
            var pdfService = new Mock<IPdfService>();
            var letterTemplate = new Mock<ILetterTemplateProvider>();

            if (!string.IsNullOrEmpty(userId))
                httpContext.User = CreatePrincipal(userId);

            var controller = new CopyrightsController(service ?? Mock.Of<ICopyrightService>(), pdfService.Object,letterTemplate.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            if (includeTempData)
                controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            if (includeUrlHelper)
            {
                var urlHelperMock = new Mock<IUrlHelper>();

                urlHelperMock.Setup(u => u.IsLocalUrl(It.IsAny<string>()))
                    .Returns<string>(url => !string.IsNullOrWhiteSpace(url) && url.StartsWith("/"));

                controller.Url = urlHelperMock.Object;
            }

            return controller;
        }

        public static ClaimsPrincipal CreatePrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth"));
        }
    }
}
