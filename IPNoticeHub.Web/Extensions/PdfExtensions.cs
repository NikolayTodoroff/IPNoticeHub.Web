using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.Implementations;
using QuestPDF.Infrastructure;

namespace IPNoticeHub.Web.Extensions
{
    public static class PdfExtensions
    {
        public static IServiceCollection AddPdfGeneration(this IServiceCollection services)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            services.AddScoped<IPdfService, PdfService>();

            return services;
        }
    }
}
