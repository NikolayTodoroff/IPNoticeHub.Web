using IPNoticeHub.Infrastructure.Persistence.Repositories.DocumentLibraryRepository;
using IPNoticeHub.Application.Repositories.DocumentLibraryRepository;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Implementations;

namespace IPNoticeHub.Web.Extensions
{
    public static class DocumentLibraryModule
    {
        public static void AddDocumentLibraryModule(this IServiceCollection services)
        {
            services.AddScoped<IDocumentLibraryRepository, DocumentLibraryRepository>();
            services.AddScoped<IDocumentLibraryService, DocumentLibraryService>();
        }
    }
}
