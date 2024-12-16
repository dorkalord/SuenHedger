using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OrderDomain;
using System.Reflection.Metadata;

namespace OrederDomainTest
{
    public class OrderServiceTest
    {
        IOrderService _orderService;

        [SetUp]
        public void Setup()
        {
            var orderBooks = new OrderBookDto[2];
            orderBooks[0] = new OrderBookDto()
            {
                AcqTime = new DateTime(2025, 1, 1, 11, 11, 11),
                Asks = new List<OrderItemDto>()
                {
                    new OrderItemDto(OrderTypeEnum.Sell, OrderKindEnum.Limit, 1.0m, 200m),
                    new OrderItemDto(OrderTypeEnum.Sell, OrderKindEnum.Limit, 1.0m, 400m),
                    new OrderItemDto(OrderTypeEnum.Sell, OrderKindEnum.Limit, 0.5m, 100m),
                    new OrderItemDto(OrderTypeEnum.Sell, OrderKindEnum.Limit, 5.0m, 200m)
                },
                Bids = new List<OrderItemDto>()
                {
                    new OrderItemDto(OrderTypeEnum.Buy, OrderKindEnum.Limit, 1.0m, 20m),
                    new OrderItemDto(OrderTypeEnum.Buy, OrderKindEnum.Limit, 1.0m, 40m),
                    new OrderItemDto(OrderTypeEnum.Buy, OrderKindEnum.Limit, 0.5m, 10m),
                    new OrderItemDto(OrderTypeEnum.Buy, OrderKindEnum.Limit, 5.0m, 20m)
                }
            };

            orderBooks[1] = new OrderBookDto()
            {
                AcqTime = new DateTime(2025, 2, 2, 22, 22, 22),
                Asks = new List<OrderItemDto>()
                {
                    new OrderItemDto(OrderTypeEnum.Sell, OrderKindEnum.Limit, 1.0m, 1200m),
                    new OrderItemDto(OrderTypeEnum.Sell, OrderKindEnum.Limit, 1.0m, 1400m),
                    new OrderItemDto(OrderTypeEnum.Sell, OrderKindEnum.Limit, 0.5m, 1100m),
                    new OrderItemDto(OrderTypeEnum.Sell, OrderKindEnum.Limit, 5.0m, 1200m)
                },
                Bids = new List<OrderItemDto>()
                {
                    new OrderItemDto(OrderTypeEnum.Buy, OrderKindEnum.Limit, 1.0m, 120m),
                    new OrderItemDto(OrderTypeEnum.Buy, OrderKindEnum.Limit, 1.0m, 140m),
                    new OrderItemDto(OrderTypeEnum.Buy, OrderKindEnum.Limit, 0.5m, 110m),
                    new OrderItemDto(OrderTypeEnum.Buy, OrderKindEnum.Limit, 5.0m, 120m)
                }
            };

            var orderBookReaderServiceMock = new Mock<IOrderBookReaderService>();
            orderBookReaderServiceMock.Setup(x => x.LoadOrderBooks()).Returns(orderBooks) ;
            _orderService = new OrderService(orderBookReaderServiceMock.Object);
        }

        [Test]
        public void SimpleBuy()
        {
            var testOrder = new RequestOrder()
            {
                Amount = 1,
                Price = 500,
                Type = OrderTypeEnum.Buy,
                Kind = OrderKindEnum.Limit
            };

            var result = _orderService.ExecuteOrder(testOrder);

            Assert.That(result.requestBookOrders.Sum(x => x.Price * x.UseAmount), Is.EqualTo(150));
        }

        [Test]
        public void SimpleSell()
        {
            var testOrder = new RequestOrder()
            {
                Amount = 2,
                Price = 100,
                Type = OrderTypeEnum.Sell,
                Kind = OrderKindEnum.Limit
            };

            var result = _orderService.ExecuteOrder(testOrder);

            Assert.That(result.requestBookOrders.Sum(x => x.UseAmount), Is.EqualTo(testOrder.Amount));
            Assert.That(result.requestBookOrders.Sum(x => x.UseAmount * x.Price), Is.EqualTo(260));
        }


        [Test]
        [TestCase(2, 500, "No viable orders found.")]
        [TestCase(500, 10, "Order could not be fulfilled in full.")]
        public void SellingOrderErrors(decimal amount, decimal price, string exceptionsMessage)
        {
            var testOrder = new RequestOrder()
            {
                Amount = amount,
                Price = price,
                Type = OrderTypeEnum.Sell,
                Kind = OrderKindEnum.Limit
            };


            var ex = Assert.Throws<Exception>(() => _orderService.ExecuteOrder(testOrder));

            Assert.That(ex.Message, Is.EqualTo(exceptionsMessage));
        }

        [Test]
        [TestCase(-5, 10, "Specified argument was out of the range of valid values. (Parameter 'Only positive numbers are accepted for Amount.')")]
        [TestCase(0.000000001, 10, "Specified argument was out of the range of valid values. (Parameter 'Theoretical minimum bitcoin order is one Satoshi or 0.00000001 BTC.')")]
        [TestCase(21000001, 10, "Specified argument was out of the range of valid values. (Parameter 'More than all bitcoins to ever exist.')")]
        public void RequestOrderErrorCases(decimal amount, decimal price, string exceptionsMessage)
        {
            var testOrder = new RequestOrder()
            {
                Amount = amount,
                Price = price,
                Type = OrderTypeEnum.Sell,
                Kind = OrderKindEnum.Limit
            };


            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _orderService.ExecuteOrder(testOrder));

            Assert.That(ex.Message, Is.EqualTo(exceptionsMessage));

        }
    }
}