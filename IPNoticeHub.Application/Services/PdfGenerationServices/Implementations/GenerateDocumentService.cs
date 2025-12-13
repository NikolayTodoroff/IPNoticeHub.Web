//using IPNoticeHub.Application.DTOs.PdfDTOs;
//using IPNoticeHub.Application.LetterComposition.Abstractions;
//using IPNoticeHub.Application.Rendering.Abstractions;
//using IPNoticeHub.Application.Repositories.LegalDocumentRepository;
//using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;

//namespace IPNoticeHub.Application.Services.PdfGenerationServices.Implementations
//{
//    public class GenerateDocumentService : IGenerateDocumentService
//    {
//        private readonly IPdfGenerator pdfGenerator;
//        private readonly ILetterAssembler inputAssembler;
//        private readonly ILegalDocumentRepository legalDocumentRepository;
//        private readonly ILegalDocumentAssembler snapshotAssembler;

//        public GenerateDocumentService(
//        IPdfGenerator pdfGenerator,
//        ILetterAssembler inputAssembler,
//        ILegalDocumentRepository legalDocumentRepository,
//        ILegalDocumentAssembler snapshotAssembler)
//        {
//            this.pdfGenerator = pdfGenerator;
//            this.inputAssembler = inputAssembler;
//            this.legalDocumentRepository = legalDocumentRepository;
//            this.snapshotAssembler = snapshotAssembler;
//        }

//        public Task<byte[]> GenerateFromInputAsync(
//            LetterInputDto input, 
//            CancellationToken ct = default)
//        {
//            var dto = inputAssembler.RebuildLetterInput(input);
//            var bytes = pdfGenerator.GenerateDocument(dto);
//            return Task.FromResult(bytes);
//        }

//        public Task<byte[]> GenerateFromSnapshotAsync(
//            Guid legalDocumentPublicId, 
//            string userId, 
//            CancellationToken ct = default)
//        {
//            var document = await legalDocumentRepository.GetByPublicIdAsync(legalDocumentPublicId, userId, ct);

//            if (document is null) throw new InvalidOperationException("Document not found.");

//            var dto = snapshotAssembler.RebuildDocumentFromSnapshot(document);
//            return pdfGenerator.GenerateDocument(dto);
//        }
//    }
//}
