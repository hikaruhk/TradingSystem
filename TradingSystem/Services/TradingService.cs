using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystem.Services
{
    public class TradingService : ITradingService
    {
        public IEnumerable<TradeOrder> GetOrders(DateTime fromDate, DateTime toDate)
        {
            return Enumerable.Empty<TradeOrder>();
        }
    }

}
