using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Trademarks.Abstractions;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Infrastructure.Persistence.Repositories.WatchlistRepository;
using IPNoticeHub.Application.Repositories.WatchlistRepository;
using IPNoticeHub.Application.Services.PdfGenerationService.Implementations;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Application.Services.TrademarkSearchService.Abstractions;
using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Application.Services.WatchlistService.Implementations;
using IPNoticeHub.Application.Templates.Abstractions;

namespace IPNoticeHub.Web.Extensions
{
    public static class TrademarkModule
    {
        public static IServiceCollection AddTrademarkModule(this IServiceCollection services)
        {
            services.AddSingleton<IStatusLabelProvider, StatusLabelProvider>();
            services.AddSingleton<ILetterTemplateProvider, LetterTemplateProvider>();

            services.AddScoped<ITrademarkRepository, TrademarkRepository>();
            services.AddScoped<ITrademarkReadRepository, TrademarkReadRepository>();
            services.AddScoped<ITrademarkStatusSnapshotRepository, TrademarkStatusSnapshotRepository>();
            services.AddScoped<IUserTrademarkRepository, UserTrademarkRepository>();
            services.AddScoped<IWatchlistRepository, WatchlistRepository>();

            services.AddScoped<ITrademarkCollectionService, TrademarkCollectionService>();
            services.AddScoped<ITrademarkSearchQueryService, TrademarkSearchQueryService>();
            services.AddScoped<ITrademarkSearchService, TrademarkSearchService>();
            services.AddScoped<IWatchlistService, WatchlistService>();

            return services;
        }
    }
}
