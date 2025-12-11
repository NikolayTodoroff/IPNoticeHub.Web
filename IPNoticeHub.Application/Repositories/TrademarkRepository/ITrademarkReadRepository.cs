using IPNoticeHub.Domain.Entities.Trademarks;

namespace IPNoticeHub.Application.Repositories.TrademarkRepository;
public interface ITrademarkReadRepository
{
    IQueryable<TrademarkEntity> TrademarkQueryNoTracking();
} 
