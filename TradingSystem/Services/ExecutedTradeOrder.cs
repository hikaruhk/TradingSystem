using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TradingSystem.Services
{
    public class ExecutedTradeOrder
    {
        public ExecutedTradeOrder() { }
        public ExecutedTradeOrder(TradeOrder order)
        {
            Id = order.Id;
            Side = order.Side;
            OrderType = order.OrderType;
            Quantity = order.Quantity;
            Price = order.Price;
            CreatedDate = order.CreatedDate;
        }

        public string SecurityCode { get; set; }
        public string Side { get; set; }
        public string OrderType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        [JsonIgnore]
        public DateTime CreatedDate { get; set; }

        [Key]
        [JsonIgnore]
        public string Id { get; set; }
    }
}
