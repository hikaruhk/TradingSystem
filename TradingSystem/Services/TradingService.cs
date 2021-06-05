using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingSystem.Datastore;

namespace TradingSystem.Services
{
    public class TradingService : ITradingService
    {
        private readonly TradingDbContext dbContext;

        public TradingService(TradingDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets incomplete orders between two dates (created dates)
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public IEnumerable<TradeOrder> GetOrders(DateTime fromDate, DateTime toDate)
        {
            var selectedOrders = dbContext
                .TradeOrders
                .Where(w => w.CreatedDate >= fromDate && w.CreatedDate < toDate && w.Quantity > 0);

            return selectedOrders;
        }

        /// <summary>
        /// Gets all orders in the system.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TradeOrder> GetOrders()
        {
            var selectedOrders = dbContext
                .TradeOrders
                .Where(w => w.Quantity > 0);

            return selectedOrders;
        }

        /// <summary>
        /// Places orders depending on the order type.
        /// </summary>
        /// <param name="newOrder"></param>
        /// <returns></returns>
        public async Task<TradeOrderResult> PlaceOrder(TradeOrder newOrder)
        {
            var availableOrders = dbContext
                .TradeOrders
                .Where(w => w.SecurityCode == newOrder.SecurityCode && newOrder.Side != w.Side)
                .OrderByDescending(o => o.CreatedDate);

            var fullfilledOrders = new List<TradeOrder>();
            var newOrderQuantity = newOrder.Quantity;
            var isIoc = newOrder.OrderType.Equals("IOC", StringComparison.InvariantCultureIgnoreCase);

            foreach (var order in availableOrders)
            {
                //We attempt to completely fullfill the order as much as we can, but only for IOC. Ignore the partial.
                if (isIoc && order.Quantity - newOrder.Quantity <= 0)
                {
                    fullfilledOrders.Add(order);
                }
                else
                {
                    //If we can fullfill the order do so, otherwise subtract the quantity and remove the fullfilled.
                    if (order.Quantity - newOrderQuantity <= 0)
                    {
                        fullfilledOrders.Add(order);
                        newOrderQuantity -= order.Quantity;
                    }
                    else
                    {
                        order.Quantity -= newOrderQuantity;
                        newOrderQuantity = 0;
                        break;
                    }
                }
            }

            newOrder.Quantity = newOrderQuantity;

            //If we cannot add fullfill anything, no need to continue (IOC)
            if (isIoc && fullfilledOrders.Count == 0)
            {
                return new TradeOrderResult
                {
                    IsSuccessful = false,
                    Message = $"No orders has been submitted."
                };
            }

            //Only add this in if the quantity matches.
            if (newOrderQuantity > 0)
            {
                dbContext
                    .TradeOrders
                    .Attach(newOrder);
                await dbContext.TradeOrders.AddAsync(newOrder);
            }

            //These were zeroed out quantities
            dbContext
                .TradeOrders
                .RemoveRange(fullfilledOrders);

            await dbContext.SaveChangesAsync();

            return new TradeOrderResult
            { 
                IsSuccessful = true,
                Message = $"Order has been submitted. {fullfilledOrders.Count} orders were completed" 
            };
        }

        /// <summary>
        /// Deletes an order by it's ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TradeOrderResult> DeleteOrder(Guid id)
        {
            var selectedOrder = await dbContext
                .TradeOrders
                .FindAsync(id.ToString());
            
            var result = selectedOrder == null
                ? new TradeOrderResult { IsSuccessful = false, Message = "Id does not exist" }
                : new TradeOrderResult { IsSuccessful = true, Message = string.Empty };

            if (result.IsSuccessful)
            {
                dbContext.TradeOrders.Remove(selectedOrder);
                await dbContext.SaveChangesAsync();
            }

            return result;
        }
    }
}
