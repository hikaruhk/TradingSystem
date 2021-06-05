using Microsoft.EntityFrameworkCore;
using TradingSystem.Services;

namespace TradingSystem.Datastore
{
    public class TradingDbContext : DbContext
    {
        public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options) { }

        public DbSet<TradeOrder> TradeOrders { get; set; }
        public DbSet<TradeOrder> ExecutedTradeOrders { get; set; }
    }
}
