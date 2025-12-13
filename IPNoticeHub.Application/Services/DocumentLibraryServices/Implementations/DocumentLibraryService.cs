using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using IPNoticeHub.Application.Repositories.DocumentLibraryRepository;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationService.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Implementations;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Shared.Enums;
using System.Globalization;
using static IPNoticeHub.Shared.Constants.DateTimeFormats.DefaultDateTimeFormat;

namespace IPNoticeHub.Application.Services.DocumentLibraryService.Implementations
{
    public class DocumentLibraryService : IDocumentLibraryService
    {
        private readonly IDocumentLibraryRepository documentRepository;
        private readonly IPdfLetterService pdfService;

        public DocumentLibraryService(
            IDocumentLibraryRepository repository,
            IPdfLetterService pdfService)
        {
            documentRepository = repository;
            this.pdfService = pdfService;
        }

        public async Task<int> SaveDocumentAsync(
            string userId, 
            DocumentCreateDto dto, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.", nameof(userId));
            } 

            if (dto is null) throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.BodyTemplate))
            {
                throw new ArgumentException(
                    "BodyTemplate cannot be null or whitespace.",
                    nameof(dto.BodyTemplate));
            }
            
            string title = string.IsNullOrWhiteSpace(dto.DocumentTitle) ? 
                GenerateDefaultTitle(dto) : dto.DocumentTitle;

            var entity = new LegalDocument
            {
                ApplicationUserId = userId,
                RelatedPublicId = dto.RelatedPublicId,
                SourceType = dto.SourceType,
                TemplateType = dto.TemplateType,
                DocumentTitle = title,
                IpTitle = dto.IpTitle,
                RegistrationNumber = dto.RegistrationNumber,
                BodyTemplate = dto.BodyTemplate,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false,

                SenderName = dto.SenderName,
                SenderAddress = dto.SenderAddress,
                SenderEmail = dto.SenderEmail,
                RecipientName = dto.RecipientName,
                RecipientAddress = dto.RecipientAddress,
                RecipientEmail = dto.RecipientEmail,
                LetterDate = dto.LetterDate,
                InfringingUrl = dto.InfringingUrl,
                GoodFaithStatement = dto.GoodFaithStatement,
                AdditionalFacts = dto.AdditionalFacts,
                YearOfCreation = dto.YearOfCreation,
                DateOfPublication = dto.DateOfPublication,
                NationOfFirstPublication = dto.NationOfFirstPublication
            };

            return await documentRepository.AddAsync(entity, cancellationToken);
        }

        public async Task<IReadOnlyList<DocumentListItemDto>> GetUserDocumentsAsync(
            string userId, 
            DocumentSourceType? sourceType = null, 
            LetterTemplateType? templateType = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.",nameof(userId));

            var documents = 
                await documentRepository.GetUserDocumentsAsync(
                userId,
                sourceType,
                templateType,
                cancellationToken);

            return documents.
                Select(d => new DocumentListItemDto
            {
                    Id = d.LegalDocumentId,
                    DocumentTitle = d.DocumentTitle,
                    SourceType = d.SourceType,
                    TemplateType = d.TemplateType,
                    IpTitle = d.IpTitle,
                    RegistrationNumber = d.RegistrationNumber,
                    CreatedOn = d.CreatedOn
                })
                .ToList();
        }

        public async Task<LegalDocument> GetSingleDocumentByIdAsync(int documentId, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.", nameof(userId));
            }

            var document = await documentRepository.GetDocumentByIdAsync(
                documentId,
                userId,
                cancellationToken);

            if (document is null) throw new InvalidOperationException(
                "Document not found or access denied.");

            return document;
        }
        public async Task<bool> RenameDocumentAsync(
            int documentId, 
            string userId, 
            string newTitle, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.", nameof(userId));

            if (documentId <= 0)
                throw new ArgumentOutOfRangeException(nameof(documentId),
                    "DocumentId must be a positive integer.");

            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException(
                    "New title cannot be null or whitespace.", nameof(newTitle));

            await documentRepository.RenameAsync(
                documentId,
                userId, 
                newTitle, 
                cancellationToken);

            return true;
        }
        public async Task<bool>DeleteDocumentAsync(
            int documentId, 
            string userId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.", nameof(userId));

            await documentRepository.SoftDeleteAsync(
                documentId, 
                userId, 
                cancellationToken);

            return true;
        }

        public async Task<(string fileName, byte[] Pdf)?> RestoreDocumentSnapshotAsync(
            int documentId, 
            string userId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                   "UserId cannot be null or whitespace.", nameof(userId));

            var document = await documentRepository.GetDocumentByIdAsync(
                documentId, 
                userId, 
                cancellationToken);

            if (document is null) return null;

            byte[] pdfBytes;

            try
            {
                pdfBytes = await pdfService.GenerateFromSavedDocumentAsync(
                    document, 
                    cancellationToken);
            }

            catch (NotSupportedException ex)
            {
                throw new InvalidOperationException(
                    $"Unsupported document combination: " +
                    $"{document.SourceType}/{document.TemplateType}", ex);
            }

            string documentTitle = string.Join(
            "_", document.DocumentTitle.Split(Path.GetInvalidFileNameChars(),
                StringSplitOptions.RemoveEmptyEntries));

            if (string.IsNullOrWhiteSpace(documentTitle))
            {
                documentTitle = "IP Infringement Notice";
            } 

            var fileName = $"{documentTitle}-{document.CreatedOn:DateTimeFormat}.pdf";

            return (fileName, pdfBytes);
        }

        private static string GenerateDefaultTitle(DocumentCreateDto dto)
        {
            string ipType = dto.TemplateType == LetterTemplateType.CeaseAndDesist ?
                "Cease & Desist" : "DMCA Notice";

            string ipTitle = string.IsNullOrWhiteSpace(dto.IpTitle) ? 
                string.Empty : $" – {dto.IpTitle}";

            string date = DateTime.UtcNow.ToString(
                DateTimeFormat,CultureInfo.InvariantCulture);

            return $"{ipType}{ipTitle} – {date}";
        }
    }
}
