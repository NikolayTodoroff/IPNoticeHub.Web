using IPNoticeHub.Domain.Entities.TrademarkRegistration;

namespace IPNoticeHub.Data.Repositories.TrademarkSearch.Abstractions;
public interface ITrademarkSearchRepository
{
    IQueryable<TrademarkEntity> TrademarkQueryNoTracking();
}
