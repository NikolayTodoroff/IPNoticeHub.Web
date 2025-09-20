using IPNoticeHub.Data.Entities.TrademarkRegistration;

public interface ITrademarkReadRepository
{
    IQueryable<TrademarkEntity> TrademarkQueryNoTracking();
}
