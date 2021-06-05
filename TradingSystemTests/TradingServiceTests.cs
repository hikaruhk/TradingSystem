using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using TradingSystem.Datastore;
using TradingSystem.Services;

namespace TradingSystemTests
{
    [TestFixture]
    public class TradingServiceTests
    {
        [Test]
        public async Task ShouldPostATradeWithSuccessfullyAsGTC()
        {
            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .Options;

            using var context = new TradingDbContext(database);

            await context.AddRangeAsync(
                new TradeOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedDate = DateTime.Now,
                    OrderType = "GTC",
                    Price = 10,
                    Quantity = 100,
                    SecurityCode = "AAPL",
                    Side = "BUY"
                });

            await context.SaveChangesAsync();

            var service = new TradingService(context);
            var newOrder = new TradeOrder
            {
                Id = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.Now,
                OrderType = "GTC",
                Price = 10,
                Quantity = 100,
                SecurityCode = "SNAP",
                Side = "BUY"
            };

            var result = await service.PlaceOrder(newOrder);

            var inserted = await context
                .TradeOrders
                .FindAsync(newOrder.Id);

            result
                .IsSuccessful
                .Should()
                .BeTrue();

            inserted.Should().NotBeNull();
        }

        [Test]
        public async Task ShouldPostATradeWithoutCompletingAsIOC()
        {
            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .Options;

            using var context = new TradingDbContext(database);

            await context.AddRangeAsync(
                new TradeOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedDate = DateTime.Now,
                    OrderType = "IOC",
                    Price = 10,
                    Quantity = 100,
                    SecurityCode = "AAPL",
                    Side = "BUY"
                });

            await context.SaveChangesAsync();

            var service = new TradingService(context);
            var newOrder = new TradeOrder
            {
                Id = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.Now,
                OrderType = "IOC",
                Price = 10,
                Quantity = 100,
                SecurityCode = "SNAP",
                Side = "BUY"
            };

            var result = await service.PlaceOrder(newOrder);

            var inserted = await context.FindAsync<TradeOrder>(newOrder.Id);

            result
                .IsSuccessful
                .Should()
                .BeFalse();

            inserted.Should().BeNull();
        }

        [Test]
        public async Task ShouldPostATradePartiallyAsGTC()
        {
            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .Options;

            using var context = new TradingDbContext(database);

            var existingOrder = Guid.NewGuid().ToString();
            await context.TradeOrders.AddRangeAsync(
                new TradeOrder
                {
                    Id = existingOrder,
                    CreatedDate = DateTime.Now,
                    OrderType = "GTC",
                    Price = 10,
                    Quantity = 100,
                    SecurityCode = "AAPL",
                    Side = "SELL"
                });

            await context.SaveChangesAsync();

            var service = new TradingService(context);
            var newOrder = new TradeOrder
            {
                Id = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.Now,
                OrderType = "GTC",
                Price = 10,
                Quantity = 101,
                SecurityCode = "AAPL",
                Side = "BUY"
            };

            var result = await service.PlaceOrder(newOrder);

            var inserted = await context.FindAsync<TradeOrder>(newOrder.Id);
            var existing = await context.FindAsync<TradeOrder>(existingOrder);

            result
                .IsSuccessful
                .Should()
                .BeTrue();

            inserted
                .Should()
                .NotBeNull()
                .And
                .BeEquivalentTo(new
                {
                    OrderType = "GTC",
                    Price = 10,
                    Quantity = 1,
                    SecurityCode = "AAPL",
                    Side = "BUY"
                });

            existing
                .Should()
                .BeNull();
        }
    }
}