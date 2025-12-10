using IPNoticeHub.Data.Repositories.Application.Implementations;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Application.TrademarkSearch.Abstractions;
using IPNoticeHub.Application.TrademarkSearch.Implementations;
using IPNoticeHub.Application.PdfGeneration.Abstractions;
using IPNoticeHub.Application.PdfGeneration.Implementations;
using IPNoticeHub.Application.Trademarks.Abstractions;
using IPNoticeHub.Application.Trademarks.Implementations;
using IPNoticeHub.Application.Watchlist.Abstractions;
using IPNoticeHub.Application.Watchlist.Implementations;
using IPNoticeHub.Data.Repositories.Watchlist.Abstractions;
using IPNoticeHub.Data.Repositories.Watchlist.Implementations;
using IPNoticeHub.Data.Repositories.TrademarkSearch.Abstractions;

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
            services.AddScoped<IUserTrademarkWatchlistRepository, UserTrademarkWatchlistRepository>();

            services.AddScoped<ITrademarkCollectionService, TrademarkCollectionService>();
            services.AddScoped<ITrademarkSearchQueryService, TrademarkSearchQueryService>();
            services.AddScoped<ITrademarkSearchService, TrademarkSearchService>();
            services.AddScoped<ITrademarkWatchlistService, TrademarkWatchlistService>();

            return services;
        }
    }
}
