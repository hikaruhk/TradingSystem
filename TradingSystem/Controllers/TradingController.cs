using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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
        [HttpGet("select")]
        public IActionResult GetOrders(
            [Required][FromHeader]DateTime fromDate,
            [Required][FromHeader]DateTime toDate) =>
                Ok(tradingService.GetOrders(fromDate, toDate));

        [Authorization(new[] { "audit", "trader" })]
        [HttpGet]
        public IActionResult GetAllOrders() =>
                Ok(tradingService.GetOrders());

        [Authorization(new[] { "trader" })]
        [HttpDelete]
        public async Task<IActionResult> DeleteOrder([Required][FromHeader]Guid id)
        {
            var result = await tradingService.DeleteOrder(id);

            if (!result.IsSuccessful) { return BadRequest(result.Message); }

            return Ok(id);
        }

        [Authorization(new[] { "trader" })]
        [HttpPost]
        public async Task<IActionResult> PostOrder([Required][FromBody]TradeOrder order)
        {
            order.CreatedDate = DateTime.UtcNow;
            order.Id = Guid.NewGuid().ToString();

            var result = await tradingService.PlaceOrder(order);

            if (!result.IsSuccessful) { return BadRequest(result.Message); }

            return Ok(result.Message);
        }
    }
}
