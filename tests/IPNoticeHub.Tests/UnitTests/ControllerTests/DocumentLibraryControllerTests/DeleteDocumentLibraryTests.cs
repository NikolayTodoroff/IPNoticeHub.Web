using FluentAssertions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.DocumentLibraryControllerTests
{
    public class DeleteDocumentLibraryTests
    {
        [Test]
        public async Task Delete_WhenUserIdMissing_ReturnsForbid_AndDoesNotCallService()
        {
            var serviceMock =
                new Mock<IDocumentLibraryService>();

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                    serviceMock,
                    userId: null);

            var result = await controller.Delete(id: 7);

            result.Should().BeOfType<ForbidResult>();

            serviceMock.Verify(
                s => s.DeleteDocumentAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Delete_WhenValid_CallsService_SetsSuccessMessage_AndRedirectsToIndex()
        {
            var serviceMock =
                new Mock<IDocumentLibraryService>();

            const int docId = 7;
            const string userId = "user-123";

            serviceMock.Setup(
                s => s.DeleteDocumentAsync(
                    docId,
                    userId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(true);

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                    serviceMock,
                    userId);

            var result = await controller.Delete(docId);

            var redirect =
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be(nameof(DocumentLibraryController.Index));
            controller.TempData.ContainsKey("SuccessMessage").Should().BeTrue();

            serviceMock.Verify(
                s => s.DeleteDocumentAsync(
                    docId,
                    userId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
