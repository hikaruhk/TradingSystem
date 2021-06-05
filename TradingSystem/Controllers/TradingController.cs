using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TradingSystem.Access;

namespace TradingSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TradingController : ControllerBase
    {

        public TradingController()
        {
        }
    }

    public class TradeOrder
    {
        public string SecurityCode { get; set; }
        public string Side { get; set; }
        public string OrderType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
