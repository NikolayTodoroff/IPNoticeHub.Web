using IPNoticeHub.Application.LetterComposition.Abstractions;
using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Implementations;
using Moq;
using NUnit.Framework;
using QuestPDF.Infrastructure;


namespace IPNoticeHub.Tests.UnitTests.ServiceTests.PdfServiceTests
{
    public class PdfLetterServiceBase
    {
        protected Mock<ILetterAssembler> letterAssembler = null!;
        protected Mock<ILegalDocumentAssembler> legalDocumentAssembler = null!;
        protected Mock<IPdfGenerator> pdfGenerator = null!;
        protected PdfLetterService service = null!;

        [SetUp]
        public void SetUp()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            letterAssembler = new Mock<ILetterAssembler>(MockBehavior.Strict);
            legalDocumentAssembler = new Mock<ILegalDocumentAssembler>(MockBehavior.Strict);
            pdfGenerator = new Mock<IPdfGenerator>(MockBehavior.Strict);

            service = new PdfLetterService(
                letterAssembler.Object,
                legalDocumentAssembler.Object,
                pdfGenerator.Object);
        }
    }
}
