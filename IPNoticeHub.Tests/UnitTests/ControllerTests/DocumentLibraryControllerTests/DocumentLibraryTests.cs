using FluentAssertions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;


namespace IPNoticeHub.Tests.UnitTests.ControllerTests.DocumentLibraryControllerTests
{
    public class DocumentLibraryTests
    {
        [Test]
        public async Task Generate_WhenUserIdMissing_ReturnsForbid()
        {
            var serviceMock = 
                new Mock<IDocumentLibraryService>();

            var controller = 
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                serviceMock,
                userId: null);

            var result = await controller.Generate(id: 42);

            result.Should().
                BeOfType<ForbidResult>();

            serviceMock.Verify(
                s => s.RestoreSavedDocumentAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Test]
        public async Task Generate_WhenSnapshotNotFound_SetsErrorMessage_AndRedirectsToIndex()
        {
            var serviceMock = 
                new Mock<IDocumentLibraryService>();

            serviceMock
                .Setup(
                s => s.RestoreSavedDocumentAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(((string? fileName, byte[]? pdf)?)null);

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                serviceMock,
                userId: "user-123");

            var result = await controller.Generate(id: 7);

            var redirect = result.Should()
                .BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be(nameof(DocumentLibraryController.Index));

            controller.TempData.ContainsKey("ErrorMessage").Should().
                BeTrue("Generate should inform the user " +
                "when the document cannot be restored.");
        }

        [Test]
        public async Task Generate_WhenSnapshotFound_ReturnsPdfFileResult()
        {
            var serviceMock = 
                new Mock<IDocumentLibraryService>();

            var expectedBytes = new byte[] { 1, 2, 3, 4 };
            const string expectedFileName = "my-letter.pdf";

            serviceMock
                .Setup(s => s.RestoreSavedDocumentAsync(
                    123,
                    "user-123",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((expectedFileName, expectedBytes));

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                serviceMock,
                userId: "user-123");

            var result = await controller.Generate(id: 123);

            var fileResult = result.Should()
                .BeOfType<FileContentResult>()
                .Subject;

            fileResult.ContentType.Should().
                Be("application/pdf");

            fileResult.FileDownloadName.Should().
                Be(expectedFileName);

            fileResult.FileContents.Should().
                BeEquivalentTo(expectedBytes);

            serviceMock.Verify(
                s => s.RestoreSavedDocumentAsync(
                    123,
                    "user-123",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Rename_WhenUserIdMissing_ReturnsForbid_AndDoesNotCallService()
        {
            var serviceMock = new Mock<IDocumentLibraryService>();

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                    serviceMock, 
                    userId: null);

            var result = await controller.Rename(
                id: 5, 
                newTitle: "New title");

            result.Should().
                BeOfType<ForbidResult>();

            serviceMock.Verify(
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
            var serviceMock = 
                new Mock<IDocumentLibraryService>();

            const int documentId = 5;
            const string userId = "user-123";
            const string newTitle = "Renamed letter";

            serviceMock
                .Setup(
                s => s.RenameDocumentAsync(
                    documentId,
                    userId,
                    newTitle,
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                    serviceMock, 
                    userId);

            var result = await controller.Rename(
                documentId, 
                newTitle);

            var redirect = result.Should()
                .BeOfType<RedirectToActionResult>()
                .Subject;

            redirect.ActionName.Should().
                Be(nameof(DocumentLibraryController.Index));

            serviceMock.Verify(
                service => service.RenameDocumentAsync(
                    documentId,
                    userId,
                    newTitle,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

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

            // assert
            result.Should().
                BeOfType<ForbidResult>();

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

            serviceMock
                .Setup(
                s => s.DeleteDocumentAsync(
                    docId,
                    userId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                    serviceMock,
                    userId);

            var result = await controller.Delete(docId);

            var redirect = result.Should()
                .BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().
                Be(nameof(DocumentLibraryController.Index));

            controller.TempData.ContainsKey("SuccessMessage").Should().
                BeTrue();

            serviceMock.Verify(
                s => s.DeleteDocumentAsync(
                    docId,
                    userId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
