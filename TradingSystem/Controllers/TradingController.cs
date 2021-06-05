using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using TradingSystem.Access;

namespace TradingSystem.Services
{
    [ApiController]
    [Route("[controller]")]
    public class TradingController : ControllerBase
    {
        private readonly ITradingService tradingService;

        public TradingController(ITradingService tradingService)
        {
            this.tradingService = tradingService;
        }


        [Authorization(new[] { "audit", "trader" })]
        [HttpGet]
        public IActionResult GetOrders(
            [Required][FromHeader]DateTime fromDate,
            [Required][FromHeader]DateTime toDate) =>
                Ok(tradingService.GetOrders(fromDate, toDate));
    }
}
