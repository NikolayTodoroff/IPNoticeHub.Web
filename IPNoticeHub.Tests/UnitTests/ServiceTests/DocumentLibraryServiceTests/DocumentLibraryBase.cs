using IPNoticeHub.Application.Repositories.DocumentLibraryRepository;
using IPNoticeHub.Application.Services.DocumentLibraryService.Implementations;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.DocumentLibraryServiceTests
{
    public class DocumentLibraryBase
    {
        protected Mock<IDocumentLibraryRepository> repository = null!;
        protected Mock<IPdfLetterService> pdfService = null!;

        protected DocumentLibraryService service = null!;

        [SetUp]
        public void SetUp()
        {
            repository = new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);
            pdfService = new Mock<IPdfLetterService>(MockBehavior.Strict);

            service = new DocumentLibraryService(repository.Object, pdfService.Object);
        }
    }
}
