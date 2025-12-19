using FluentAssertions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.DocumentLibraryControllerTests
{
    public class GenerateDocumentLibraryTests
    {
        [Test]
        public async Task Generate_WhenUserIdMissing_ReturnsForbid()
        {
            var service = 
                new Mock<IDocumentLibraryService>();

            var controller = 
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId: null);

            var result = await controller.Generate(id: 42);

            result.Should().BeOfType<ForbidResult>();

            service.Verify(
                s => s.RestoreSavedDocumentAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Test]
        public async Task Generate_WhenSnapshotNotFound_AndRedirectsToIndex_WithMessage()
        {
            var service = 
                new Mock<IDocumentLibraryService>();

            service.Setup(
                s => s.RestoreSavedDocumentAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(((string fileName, byte[] pdf)?)null);

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId: "user-123");

            var result = await controller.Generate(id: 7);

            var redirect = result.
                Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be(nameof(DocumentLibraryController.Index));

            controller.TempData.ContainsKey("ErrorMessage").Should().
                BeTrue("Generate should inform the user " +
                "when the document cannot be restored.");
        }

        [Test]
        public async Task Generate_WhenSnapshotFound_ReturnsPdfFileResult()
        {
            var service = 
                new Mock<IDocumentLibraryService>();

            var expectedBytes = new byte[] { 1, 2, 3, 4 };
            const string expectedFileName = "my-letter.pdf";

            service.Setup(
                s => s.RestoreSavedDocumentAsync(
                    123,
                    "user-123",
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((expectedFileName, expectedBytes));

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId: "user-123");

            var result = await controller.Generate(id: 123);

            var fileResult = 
                result.Should().BeOfType<FileContentResult>().Subject;

            fileResult.ContentType.Should().Be("application/pdf");
            fileResult.FileDownloadName.Should().Be(expectedFileName);
            fileResult.FileContents.Should().BeEquivalentTo(expectedBytes);

            service.Verify(
                s => s.RestoreSavedDocumentAsync(
                    123,
                    "user-123",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
