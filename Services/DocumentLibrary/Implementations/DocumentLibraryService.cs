using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.LegalDocuments;
using IPNoticeHub.Data.Repositories.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.DTOs;
using System.ComponentModel.Design;
using System.Globalization;
using static IPNoticeHub.Common.ValidationConstants.FormattingConstants;
namespace IPNoticeHub.Services.DocumentLibrary.Implementations
{
    public class DocumentLibraryService : IDocumentLibraryService
    {
        private readonly IDocumentLibraryRepository documentRepository;
        private readonly IPdfService pdfService;

        public DocumentLibraryService(
            IDocumentLibraryRepository repository,
            IPdfService pdfService)
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
                UserId = userId,
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
                    Id = d.Id,
                    DocumentTitle = d.DocumentTitle,
                    SourceType = d.SourceType,
                    TemplateType = d.TemplateType,
                    IpTitle = d.IpTitle,
                    RegistrationNumber = d.RegistrationNumber,
                    CreatedOn = d.CreatedOn
                })
                .ToList();
        }

        public async Task<bool>RenameDocumentAsync(
            int documentId, 
            string userId, 
            string newTitle, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.", nameof(userId));

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

            if (document.TemplateType == LetterTemplateType.CeaseAndDesist)
            {
                var input = new CeaseDesistInput(
                    SenderName: document.SenderName,
                    SenderAddress: document.SenderAddress,
                    RecipientName: document.RecipientName,
                    RecipientAddress: document.RecipientAddress,
                    Date: document.LetterDate,
                    WorkTitle: document.IpTitle ?? string.Empty,
                    RegistrationNumber: document.RegistrationNumber ?? string.Empty,
                    AdditionalFacts: document.AdditionalFacts,
                    BodyTemplate: document.BodyTemplate
                );

                if (document.SourceType == DocumentSourceType.Trademark)
                {
                    pdfBytes = await pdfService.GenerateTrademarkCeaseDesistAsync(
                        input, cancellationToken);
                }

                else
                {
                    pdfBytes = await pdfService.GenerateCopyrightCeaseDesistAsync(
                        input,  cancellationToken);
                }
            }

            else if (document.TemplateType == LetterTemplateType.Dmca)
            {
                var input = new DMCAInput(
                    SenderName: document.SenderName,
                    SenderEmail: document.SenderEmail ?? string.Empty,
                    SenderAddress: document.SenderAddress,
                    RecipientName: document.RecipientName,
                    RecipientEmail: document.RecipientEmail ?? string.Empty,
                    RecipientAddress: document.RecipientAddress,
                    Date: document.LetterDate,
                    WorkTitle: document.IpTitle ?? string.Empty,
                    RegistrationNumber: document.RegistrationNumber ?? string.Empty,
                    YearOfCreation: document.YearOfCreation,
                    DateOfPublication: document.DateOfPublication,
                    NationOfFirstPublication: document.NationOfFirstPublication,
                    InfringingUrl: document.InfringingUrl ?? string.Empty,
                    GoodFaithStatement: document.GoodFaithStatement ?? string.Empty,
                    BodyTemplate: document.BodyTemplate
                );

                pdfBytes = await pdfService.GenerateCopyrightDMCAAsync(input, cancellationToken);
            }

            else
            {
                throw new InvalidOperationException(
                    $"Unsupported document combination: " +
                    $"{document.SourceType} / {document.TemplateType}");
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
