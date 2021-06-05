namespace TradingSystem.Services
{
    public class TradeOrder
    {
        public string SecurityCode { get; set; }
        public string Side { get; set; }
        public string OrderType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}
