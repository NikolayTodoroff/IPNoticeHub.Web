using FluentAssertions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.DocumentLibraryControllerTests
{
    public class RenameDocumentLibraryTests
    {
        [Test]
        public async Task Rename_WhenUserIdMissing_ReturnsForbid_AndDoesNotCallService()
        {
            var service = new Mock<IDocumentLibraryService>();

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                    service,
                    userId: null);

            var result = await controller.Rename(
                id: 5,
                newTitle: "New title");

            result.Should().BeOfType<ForbidResult>();

            service.Verify(
                s => s.RenameDocumentAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Rename_WhenValid_CallsService_SetsSuccessMessage_AndRedirectsToIndex()
        {
            var service =
                new Mock<IDocumentLibraryService>();

            const int documentId = 5;
            const string userId = "user-123";
            const string newTitle = "Renamed letter";

            service.Setup(
                s => s.RenameDocumentAsync(
                    documentId,
                    userId,
                    newTitle,
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                    service,
                    userId);

            var result = await controller.Rename(
                documentId,
                newTitle);

            var redirect =
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be(nameof(DocumentLibraryController.Index));

            service.Verify(
                service => service.RenameDocumentAsync(
                    documentId,
                    userId,
                    newTitle,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Rename_WithNullTitle_ReturnsRedirectWithErrorMessage()
        {
            var serviceMock =
                new Mock<IDocumentLibraryService>();

            var documentId = 1;
            const string userId = "user-123";
            string nullNewTitle = null!;

            serviceMock.Setup(
                s => s.DeleteDocumentAsync(
                    documentId,
                    userId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(true);

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                    serviceMock,
                    userId);

            var result = await controller.Rename(documentId, nullNewTitle!);

            var redirect =
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be(nameof(DocumentLibraryController.Index));

            controller.TempData["ErrorMessage"].Should().Be("Title cannot be empty.");

            serviceMock.Verify(
                s => s.RenameDocumentAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
