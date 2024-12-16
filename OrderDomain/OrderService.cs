using System.Collections.Generic;

namespace OrderDomain
{
    public class OrderService : IOrderService
    {
        private readonly OrderBookDto[] _orderBooks;

        public OrderService(IOrderBookReaderService orderBookReaderService)
        {
            _orderBooks = orderBookReaderService.LoadOrderBooks();
        }

        public OrderResponse ExecuteOrder(RequestOrder requestOrder)
        {
            ValidateOrder(requestOrder);

            Dictionary<decimal, List<BookOrder>> availableBookOrders = GetGoodOrdersForRequest(requestOrder);

            if (!availableBookOrders.Any())
            {
                throw new Exception("No viable orders found.");
            }

            var result = requestOrder.Type switch
            {
                OrderTypeEnum.Buy => Buy(availableBookOrders, requestOrder),
                OrderTypeEnum.Sell => Sell(availableBookOrders, requestOrder),
                _ => throw new NotImplementedException()
            };

            if (requestOrder.Amount > result.requestBookOrders.Sum(x => x.UseAmount))
            {
                throw new Exception("Order could not be fulfilled in full.");
            }

            return result;
        }

        private Dictionary<decimal, List<BookOrder>> GetGoodOrdersForRequest(RequestOrder requestOrder)
        {
            Dictionary<decimal, List<BookOrder>> availableBookOrders = [];
            foreach (var orderBook in _orderBooks)
            {
                var bookOrdersWithinRequestOrderPrice = requestOrder.Type switch
                {
                    OrderTypeEnum.Buy => orderBook.Asks.Select(x => x.Order).Where(x => x.Price <= requestOrder.Price),
                    OrderTypeEnum.Sell => orderBook.Bids.Select(x => x.Order).Where(x => x.Price >= requestOrder.Price),
                    _ => throw new NotImplementedException()
                };

                foreach (var order in bookOrdersWithinRequestOrderPrice)
                {
                    if (!availableBookOrders.ContainsKey(order.Price))
                    {
                        availableBookOrders.Add(order.Price, []);
                    }

                    availableBookOrders[order.Price].Add(new() { Amount = order.Amount, Price = order.Price, ExchangeId = orderBook.AcqTime.ToLongDateString() });
                }
            }

            return availableBookOrders;
        }

        private void ValidateOrder(RequestOrder order)
        {
            if (order.Amount < 0.0m)
            {
                throw new ArgumentOutOfRangeException("Only positive numbers are accepted for Amount.");
            }
            if (order.Amount < 0.00000001m)
            {
                throw new ArgumentOutOfRangeException("Theoretical minimum bitcoin order is one Satoshi or 0.00000001 BTC.");
            }
            if (order.Amount > 21000000m)
            {
                throw new ArgumentOutOfRangeException("More than all bitcoins to ever exist.");
            }
            if (order.Price < 0.0m)
            {
                throw new ArgumentOutOfRangeException("Only positive numbers are accepted for Price.");
            }
            if (order.Type != OrderTypeEnum.Buy && order.Type != OrderTypeEnum.Sell)
            {
                throw new ArgumentOutOfRangeException("Order Type not supported, use only Buy or Sell");
            }
            if (order.Kind != OrderKindEnum.Limit )
            {
                throw new NotImplementedException("Only Limit order kind supported");
            }
        }


        private static OrderResponse Sell(Dictionary<decimal, List<BookOrder>> availableBookOrders, RequestOrder requestOrder)
        {
            var bitcoinsToSell = requestOrder.Amount;
            var response = new OrderResponse(requestOrder);

            foreach (var orders in availableBookOrders.OrderByDescending(x => x.Key).Select(x => x.Value))
            {
                foreach (var order in orders.OrderByDescending(x => x.Amount))
                {
                    bitcoinsToSell -= order.Amount;
                    if (bitcoinsToSell >= 0.0m)
                    {
                        Console.WriteLine($"Sell in full to book order {order.Amount} @ {order.Price}, from exchange {order.ExchangeId}");

                        response.requestBookOrders.Add(new RequestBookOrder() { Amount = order.Amount, ExchangeId = order.ExchangeId, Price = order.Price, UseAmount = order.Amount });
                        if (bitcoinsToSell == 0)
                        {
                            break;
                        }
                    }
                    else if (bitcoinsToSell < 0)
                    {
                        var partial = requestOrder.Amount - response.requestBookOrders.Sum(x => x.Amount);
                        Console.WriteLine($"Partial sell {order.Amount + bitcoinsToSell} to book order {order.Amount} @ {order.Price}, from exchange {order.ExchangeId}");
                        response.requestBookOrders.Add(new RequestBookOrder() { Amount = order.Amount, ExchangeId = order.ExchangeId, Price = order.Price, UseAmount = partial });
                        break;
                    }
                }
                if (bitcoinsToSell < 0)
                {
                    break;
                }
            }
            return response;
        }


        private static OrderResponse Buy(Dictionary<decimal, List<BookOrder>> availableBookOrders, RequestOrder requestOrder)
        {
            decimal currentSum = 0.0m;
            var response = new OrderResponse(requestOrder);

            foreach (var orders in availableBookOrders.OrderBy(x => x.Key).Select(x => x.Value))
            {
                foreach (var order in orders.OrderByDescending(x => x.Amount))
                {
                    currentSum += order.Amount;
                    if (currentSum <= requestOrder.Amount)
                    {
                        Console.WriteLine($"Buy full book order {order.Amount} @ {order.Price}, from exchange {order.ExchangeId}");

                        response.requestBookOrders.Add(new RequestBookOrder() { Amount = order.Amount, ExchangeId = order.ExchangeId, Price = order.Price, UseAmount = order.Amount });
                        if (currentSum == requestOrder.Amount)
                        {
                            break;
                        }
                    }
                    else if (currentSum > requestOrder.Amount)
                    {
                        var partial = requestOrder.Amount - response.requestBookOrders.Sum(x => x.Amount);
                        Console.WriteLine($"Partial buy {partial} book order {order.Amount} @ {order.Price}, from exchange {order.ExchangeId}");
                        response.requestBookOrders.Add(new RequestBookOrder() { Amount = order.Amount, ExchangeId = order.ExchangeId, Price = order.Price, UseAmount = partial });
                        break;
                    }
                }
                if (currentSum >= requestOrder.Amount)
                {
                    break;
                }
            }

            return response;
        }
    }
}
