using FluentValidation;
using System.Text.RegularExpressions;

namespace TradingSystem.Services
{
    /// <summary>
    /// Validators for the incoming orders. This will take a look at the actual values rather than if
    /// they are there or not.
    /// </summary>
    public class TradeOrderValidator : AbstractValidator<TradeOrder>
    {
        public TradeOrderValidator()
        {
            RuleFor(r => r.SecurityCode)
                .Matches("AA(10|0[1-9])", RegexOptions.IgnoreCase)
                .WithMessage("Not within the trading universe!");

            RuleFor(r => r.Side)
                .Matches("(BUY|SELL)", RegexOptions.IgnoreCase)
                .WithMessage("Only buy or sell is allowed");

            RuleFor(r => r.OrderType)
                .Matches("(IOC|GTC)", RegexOptions.IgnoreCase)
                .WithMessage("Only IOC and GTC is allowed");

            RuleFor(r => r.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity has to be greater than 0");

            RuleFor(r => r.Price)
                .GreaterThan(0)
                .WithMessage("Price has to be greater than 0");
        }
    }

}
