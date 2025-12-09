using IPNoticeHub.Data.Repositories.DocumentLibrary.Abstractions;
using IPNoticeHub.Data.Repositories.DocumentLibrary.Implementations;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.Implementations;

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
