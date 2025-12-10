using IPNoticeHub.Application.DocumentLibrary.Abstractions;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.UnitTestUtilities
{
    public class DocumentLibraryControllerFactory
    {
        public static DocumentLibraryController CreateDocumentLibraryController(
            Mock<IDocumentLibraryService> serviceMock,
            string? userId)
        {
            var controller =
                new DocumentLibraryController(serviceMock.Object);

            var httpContext = new DefaultHttpContext();

            if (userId is not null)
            {
                var identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    },
                    "TestAuth");

                httpContext.User = new ClaimsPrincipal(identity);
            }
            else
            {
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());

            return controller;
        }
    }
}
