using FluentAssertions;
using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.DocumentLibraryControllerTests
{
    public class IndexDocumentLibraryTests
    {
        [Test]
        public async Task Index_WhenUserIdPresent_CallsServiceWithCorrectParameters()
        {
            var serviceMock =
                new Mock<IDocumentLibraryService>();

            const string userId = "user-123";

            var expectedDocuments =
                new List<DocumentListItemDto>
            {
                new DocumentListItemDto {
                    Id = 1,
                    DocumentTitle = "Test Document 1" },

                new DocumentListItemDto {
                    Id = 2,
                    DocumentTitle = "Test Document 2" }
            };

            serviceMock.Setup(
                s => s.GetUserDocumentsAsync(
                    userId,
                    null,
                    null,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(expectedDocuments);

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                serviceMock,
                userId);

            var result =
                await controller.Index(null, null);

            var viewResult =
                result.Should().BeOfType<ViewResult>().Subject;

            var dto =
                viewResult.Model.Should().
                BeAssignableTo<IReadOnlyList<DocumentListItemDto>>().Subject;

            dto.Should().HaveCount(2);
            dto.Should().BeEquivalentTo(expectedDocuments);

            serviceMock.Verify(
                s => s.GetUserDocumentsAsync(
                    userId,
                    null,
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Index_WithSourceTypeFilter_PassesSourceTypeToService()
        {
            var serviceMock =
                new Mock<IDocumentLibraryService>();

            const string userId = "user-456";
            var sourceType = DocumentSourceType.Trademark;

            serviceMock.Setup(
                s => s.GetUserDocumentsAsync(
                    userId,
                    sourceType,
                    null,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(new List<DocumentListItemDto>());

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                serviceMock,
                userId);

            await controller.Index(sourceType, null);

            serviceMock.Verify(
                s => s.GetUserDocumentsAsync(
                    userId,
                    sourceType,
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Index_WithTemplateTypeFilter_PassesTemplateTypeToService()
        {
            var serviceMock =
                new Mock<IDocumentLibraryService>();

            const string userId = "user-789";

            var templateType = LetterTemplateType.CeaseAndDesist;

            serviceMock.Setup(
                s => s.GetUserDocumentsAsync(
                    userId,
                    null,
                    templateType,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(new List<DocumentListItemDto>());

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                serviceMock,
                userId);

            await controller.Index(null, templateType);

            serviceMock.Verify(
                s => s.GetUserDocumentsAsync(
                    userId,
                    null,
                    templateType,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Index_WithBothFilters_PassesBothToService()
        {
            var serviceMock =
                new Mock<IDocumentLibraryService>();

            const string userId = "user-111";

            var sourceType = DocumentSourceType.Copyright;
            var templateType = LetterTemplateType.Dmca;

            serviceMock.Setup(
                s => s.GetUserDocumentsAsync(
                    userId,
                    sourceType,
                    templateType,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(new List<DocumentListItemDto>());

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                serviceMock,
                userId);

            await controller.Index(sourceType, templateType);

            serviceMock.Verify(
                s => s.GetUserDocumentsAsync(
                    userId,
                    sourceType,
                    templateType,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Index_WhenServiceReturnsEmptyList_ReturnsViewWithEmptyModel()
        {
            var serviceMock =
                new Mock<IDocumentLibraryService>();

            const string userId = "user-empty";

            serviceMock.Setup(
                s => s.GetUserDocumentsAsync(
                    userId,
                    null,
                    null,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(new List<DocumentListItemDto>());

            var controller =
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                serviceMock,
                userId);

            var result =
                await controller.Index(null, null);

            var viewResult =
                result.Should().BeOfType<ViewResult>().Subject;

            var model =
                viewResult.Model.Should().
                BeAssignableTo<IReadOnlyList<DocumentListItemDto>>().Subject;

            model.Should().BeEmpty();
        }
    }
}
