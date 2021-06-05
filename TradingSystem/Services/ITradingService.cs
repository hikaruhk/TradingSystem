using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TradingSystem.Services
{
    public interface ITradingService
    {
        IEnumerable<ExecutedTradeOrder> GetOrders(DateTime fromDate, DateTime toDate);
        IEnumerable<TradeOrder> GetOrders();
        Task<TradeOrderResult> DeleteOrder(Guid id);
        Task<TradeOrderResult> PlaceOrder(TradeOrder newOrder);
    }

}
