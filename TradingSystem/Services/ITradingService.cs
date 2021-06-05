using System;
using System.Collections.Generic;

namespace TradingSystem.Services
{
    public interface ITradingService
    {
        IEnumerable<TradeOrder> GetOrders(DateTime fromDate, DateTime toDate);
    }

}
