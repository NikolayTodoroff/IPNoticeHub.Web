using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.LetterComposition.Abstractions;
using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Domain.Entities.LegalDocuments;

namespace IPNoticeHub.Application.Services.PdfGenerationServices.Implementations
{
    public class PdfLetterService : IPdfLetterService
    {
        private readonly ILetterAssembler letterAssembler;
        private readonly ILegalDocumentAssembler legalDocumentAssembler;
        private readonly IPdfGenerator pdfGenerator;

        public PdfLetterService(
            ILetterAssembler letterAssembler,
            ILegalDocumentAssembler legalDocumentAssembler,
            IPdfGenerator pdfGenerator)
        {
            this.letterAssembler = letterAssembler;
            this.legalDocumentAssembler = legalDocumentAssembler;
            this.pdfGenerator = pdfGenerator;
        }

        public Task<byte[]> GenerateFromInputAsync(LetterInputDto input, CancellationToken cancellationToken = default)
        {
            var dto = letterAssembler.RebuildLetterInput(input);
            return Task.FromResult(pdfGenerator.GenerateDocument(dto));
        }

        public Task<byte[]> GenerateFromSavedDocumentAsync(LegalDocument document, CancellationToken cancellationToken = default)
        {
            var dto = legalDocumentAssembler.RebuildFromSavedDocument(document);
            return Task.FromResult(pdfGenerator.GenerateDocument(dto));
        }
    }
}
