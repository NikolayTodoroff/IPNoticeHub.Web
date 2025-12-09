using IPNoticeHub.Data.Repositories.Application.Abstractions;
using IPNoticeHub.Data.Repositories.Application.Implementations;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.Extensions;
using IPNoticeHub.Services.Application.Implementations;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.Implementations;

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
