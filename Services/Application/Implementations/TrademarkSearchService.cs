using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPNoticeHub.Services.Application.Implementations
{
    public sealed class TrademarkSearchService : ITrademarkSearchService
    {
        private readonly ITrademarkReadRepository tmSearchServiceRepo;

        public TrademarkSearchService(ITrademarkReadRepository tmSearchServiceRepository)
        {
            this.tmSearchServiceRepo = tmSearchServiceRepository;
        }

        public async Task<(IReadOnlyList<TrademarkSearchResultDTO> Items, int Total)>
            SearchAsync(TrademarkSearchQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
