using IPNoticeHub.Application.LetterComposition.Abstractions;
using IPNoticeHub.Application.LetterComposition.Implementations;
using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationService.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationService.Implementations;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Infrastructure.Rendering;
using IPNoticeHub.Infrastructure.Rendering.Implementation;
using QuestPDF.Infrastructure;

namespace IPNoticeHub.Web.Extensions
{
    public static class PdfExtensions
    {
        public static IServiceCollection AddPdfGeneration(this IServiceCollection services)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            services.AddScoped<IPdfGenerator, QuestPdfGenerator>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<ILetterTemplateProvider, LetterTemplateProvider>();
            services.AddScoped<ITemplateTokenReplacer, RegexTemplateTokenReplacer>();
            services.AddScoped<ILetterAssembler, LetterAssembler>();
            services.AddScoped<ILegalDocumentAssembler, LegalDocumentAssembler>();

            return services;
        }
    }
}
