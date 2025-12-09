using IPNoticeHub.Data.Repositories.Application.Abstractions;
using IPNoticeHub.Data.Repositories.Application.Implementations;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Services.TrademarkSearch.Abstractions;
using IPNoticeHub.Services.TrademarkSearch.Implementations;
using IPNoticeHub.Services.PdfGeneration.Abstractions;
using IPNoticeHub.Services.PdfGeneration.Implementations;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.Implementations;
using IPNoticeHub.Services.Watchlist.Abstractions;
using IPNoticeHub.Services.Watchlist.Implementations;

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
