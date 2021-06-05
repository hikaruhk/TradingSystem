using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .UseInternalServiceProvider(serviceProvider)
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
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .UseInternalServiceProvider(serviceProvider)
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
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .UseInternalServiceProvider(serviceProvider)
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

        [Test]
        public async Task ShouldRetrieveExecutedOrders()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using var context = new TradingDbContext(database);

            var existingOrder = Guid.NewGuid().ToString();
            await context.ExecutedTradeOrders.AddRangeAsync(
                new ExecutedTradeOrder
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

            var orders = service.GetOrders(DateTime.MinValue, DateTime.MaxValue);

            orders
                .Should()
                .BeEquivalentTo(new[] { new ExecutedTradeOrder
                {
                    Id = existingOrder,
                    CreatedDate = DateTime.Now,
                    OrderType = "GTC",
                    Price = 10,
                    Quantity = 100,
                    SecurityCode = "AAPL",
                    Side = "SELL" 
                }}, options => options.Excluding(o => o.CreatedDate));
        }

        [Test]
        public async Task ShouldNotRetrieveExecutedOrders()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using var context = new TradingDbContext(database);

            var existingOrder = Guid.NewGuid().ToString();
            await context.ExecutedTradeOrders.AddRangeAsync(
                new ExecutedTradeOrder
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

            var orders = service.GetOrders(DateTime.MinValue, DateTime.MinValue);

            orders
                .Should()
                .BeEmpty();
        }

        [Test]
        public async Task ShouldRetrieveOrders()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .UseInternalServiceProvider(serviceProvider)
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

            var orders = service.GetOrders();

            orders
                .Should()
                .BeEquivalentTo(new[] { new ExecutedTradeOrder
                {
                    Id = existingOrder,
                    CreatedDate = DateTime.Now,
                    OrderType = "GTC",
                    Price = 10,
                    Quantity = 100,
                    SecurityCode = "AAPL",
                    Side = "SELL"
                }}, options => options.Excluding(o => o.CreatedDate));
        }

        [Test]
        public async Task ShouldDeleteOrder()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using var context = new TradingDbContext(database);

            var existingOrder = Guid.NewGuid();
            await context.TradeOrders.AddRangeAsync(
                new TradeOrder
                {
                    Id = existingOrder.ToString(),
                    CreatedDate = DateTime.Now,
                    OrderType = "GTC",
                    Price = 10,
                    Quantity = 0,
                    SecurityCode = "AAPL",
                    Side = "SELL"
                });

            await context.SaveChangesAsync();

            var service = new TradingService(context);

            var orders = await service.DeleteOrder(existingOrder);

            orders.IsSuccessful.Should().BeTrue();
            var deleted = await context
                .TradeOrders
                .FindAsync(existingOrder.ToString());

            deleted.Should().BeNull();
        }

        [Test]
        public async Task ShouldNotDeleteOrder()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var database = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: "tradeSystem")
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using var context = new TradingDbContext(database);

            var existingOrder = Guid.NewGuid();
            await context.TradeOrders.AddRangeAsync(
                new TradeOrder
                {
                    Id = existingOrder.ToString(),
                    CreatedDate = DateTime.Now,
                    OrderType = "GTC",
                    Price = 10,
                    Quantity = 0,
                    SecurityCode = "AAPL",
                    Side = "SELL"
                });

            await context.SaveChangesAsync();

            var service = new TradingService(context);

            var orders = await service.DeleteOrder(Guid.Empty);

            orders.IsSuccessful.Should().BeFalse();

            var deleted = await context
                .TradeOrders
                .FindAsync(existingOrder.ToString());

            deleted.Should().NotBeNull();
        }
    }
}