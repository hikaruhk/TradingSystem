using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using TradingSystem.Services;

namespace TradingSystemTests
{
    [TestFixture]
    public class TradeOrderValidatorTests
    {
        [TestCase("IOC", 10, 10, "AA10", "BUY", ExpectedResult = true, Description = "IOC Valid model")]
        [TestCase("ioc", 10, 10, "AA10", "BUY", ExpectedResult = true, Description = "IOC Valid model, casing")]
        [TestCase("GTC", 10, 10, "AA10", "SELL", ExpectedResult = true, Description = "GTC Valid model")]
        [TestCase("GTC", 10, 10, "AA01", "BUY", ExpectedResult = true, Description = "GTC Valid model security code")]
        [TestCase("ABC", 10, 10, "AA10", "BUY", ExpectedResult = false, Description = "order type invalid model")]
        [TestCase("GTC", 0, 10, "AA10", "BUY", ExpectedResult = false, Description = "price invalid model")]
        [TestCase("GTC", 10, 0, "AA10", "BUY", ExpectedResult = false, Description = "quantity invalid model")]
        [TestCase("GTC", 10, 10, "BB10", "BUY", ExpectedResult = false, Description = "securityCode invalid model")]
        [TestCase("GTC", 10, 10, "AA10", "ASDF", ExpectedResult = false, Description = "side invalid model")]
        public async Task<bool> ShouldValidateTradeOrder(
            string orderType,
            decimal price,
            int quantity,
            string securityCode,
            string side)
        {
            var validator = new TradeOrderValidator();
            var tradeOrder = new TradeOrder
            {
                OrderType = orderType,
                Price = price,
                Quantity = quantity,
                SecurityCode = securityCode,
                Side = side
            };

            var result = await validator.ValidateAsync(tradeOrder);

            return result.IsValid;
        }
    }
}