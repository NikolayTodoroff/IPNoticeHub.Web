using IPNoticeHub.Data.Entities.TrademarkRegistration;

namespace IPNoticeHub.Data.Repositories.TrademarkSearch.Abstractions;
public interface ITrademarkReadRepository
{
    IQueryable<TrademarkEntity> TrademarkQueryNoTracking();
}
