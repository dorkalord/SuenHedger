using System.Text.Json;
using System.Text.Json.Serialization;

public class OrderItem
{
    public Order Order { get; set; }
    public override string ToString()
    {
        return $"order {Order.Type} amount: {Order.Amount}, order price: {Order.Price}";
    }
}

public class Order
{
    public object Id { get; set; }
    public DateTime Time { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderType Type { get; set; }
    public string Kind { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }

    public override string ToString() {
        return $"order amount: {Amount}, order price: {Price}";
    }
}

public class OrderBook
{
    public DateTime AcqTime { get; set; }
    /// <summary>
    /// Available buyers
    /// </summary>
    public List<OrderItem> Bids { get; set; }
    /// <summary>
    /// Available sellers
    /// </summary>
    public List<OrderItem> Asks { get; set; }

    public decimal BestMaxBidPrice { get; set; }
    public decimal BestMinAskPrice { get; set; }

    public override string ToString()
    {
        return $"Best buy price: {BestMaxBidPrice}, best sell price: {BestMinAskPrice}";
    }
}

public enum OrderType { Buy, Sell };

class Program
{
    static void Main()
    {
        var orderBooks = LoadOrderBooks();
        var requestedOrder = FillOutOrder();

        //var requestedOrder = new Order()
        //{
        //    Amount = 10,
        //    Price = 2955,
        //    Type = OrderType.Buy ,
        //    Kind = "Limit"
        //};

        Console.WriteLine($"Processing order {requestedOrder}");

        switch (requestedOrder.Type)
        {
            case OrderType.Buy:
                Buy(orderBooks, requestedOrder);
                break;
            case OrderType.Sell:
                Sell(orderBooks, requestedOrder);
                break;
        }
    }

    private static OrderBook[] LoadOrderBooks()
    {
        var lines = File.ReadAllLines("order_books_data - origin");
        //var lines = File.ReadAllLines("order_books_data");
        OrderBook[] orderBooks = new OrderBook[lines.Length];
        int j = 0;
        foreach (var line in lines)
        {
            if (line.Length > 1)
            {
                var jsonStart = line.IndexOf('{') - 1;
                OrderBook? orderBook = JsonSerializer.Deserialize<OrderBook>(line[jsonStart..]);

                if (orderBook is null)
                {
                    throw new Exception("Problem reading order book.");
                }

                orderBook.Asks.OrderBy(x => x.Order.Price);
                orderBook.Bids.OrderByDescending(x => x.Order.Price);

                orderBook.BestMinAskPrice = orderBook.Asks.First().Order.Price;
                orderBook.BestMaxBidPrice = orderBook.Bids.First().Order.Price;
                orderBooks[j] = orderBook;
                j++;
            }
        }

        return orderBooks;
    }

    private static Order FillOutOrder()
    {
        Console.WriteLine("Buy or sel BTC? ");

        OrderType orderType = OrderType.Buy;

        (int left, int top) = Console.GetCursorPosition();
        var decorator = "✅  ";
        ConsoleKeyInfo key;
        bool isSelected = false;

        while (!isSelected)
        {
            Console.SetCursorPosition(left, top);

            Console.WriteLine($"{(orderType == OrderType.Buy ? decorator : "    ")}Buy ");
            Console.WriteLine($"{(orderType == OrderType.Sell ? decorator : "    ")}Sell");

            key = Console.ReadKey(false);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    orderType = OrderType.Buy;
                    break;

                case ConsoleKey.DownArrow:
                    orderType = OrderType.Sell;
                    break;

                case ConsoleKey.Enter:
                    isSelected = true;
                    break;
            }
        }

        Console.WriteLine($"How much BTC would you like to {orderType} : ");
        decimal amount = 0.0m;
        var isValidDecimal = decimal.TryParse(Console.ReadLine(), out amount);

        if (!isValidDecimal)
        {
            throw new InvalidCastException("We could not parse the provided value.");
        }
        else if (amount < 0.0m)
        {
            throw new ArgumentOutOfRangeException("Only positive numbers are accepted.");
        }
        else if (amount < 0.00000001m)
        {
            throw new ArgumentOutOfRangeException("Theoretical minimum bitcoin order is one Satoshi or 0.00000001 BTC.");
        }
        else if (amount > 21000000m)
        {
            throw new ArgumentOutOfRangeException("More than all bitcoins to ever exist.");
        }


        switch (orderType)
        {
            case OrderType.Buy:
                Console.WriteLine($"What is he maximum you are willing to pay per one BTC : ");
                break;
            case OrderType.Sell:
                Console.WriteLine($"What is he minimum you are willing to sell per one BTC : ");
                break;
        }
        decimal limit = 0.0m;
        isValidDecimal = decimal.TryParse(Console.ReadLine(), out limit);

        if (!isValidDecimal)
        {
            throw new InvalidCastException("We could not parse the provided value.");
        }
        else if (amount < 0.0m)
        {
            throw new ArgumentOutOfRangeException("Only positive numbers are accepted.");
        }


        Console.WriteLine($"Order {orderType} for {amount} with limit {limit} is now processing...");




        var customerOrder = new Order()
        {
            Amount = amount,
            Price = limit,
            Type = orderType,
            Kind = "Limit"
        };
        return customerOrder;
    }


    private static void Sell(OrderBook[] orderBooks, Order requestedOrder)
    {
        var bitcoinsToSell = requestedOrder.Amount;
        var countOfOrders = 0;

        for (int i = 0; bitcoinsToSell >= 0  && i < orderBooks.Count(); i++)
        {
            Console.WriteLine($"Processing order book from {orderBooks[i].AcqTime}");
            if (orderBooks[i].BestMaxBidPrice >= requestedOrder.Price)
            {
                var ordersToFulfill = HandleSellOrder(requestedOrder, orderBooks[i].Bids, bitcoinsToSell);
                countOfOrders += ordersToFulfill.Count;
                bitcoinsToSell -= ordersToFulfill.Sum(x => x.Amount);
            }
        }

        if (countOfOrders == 0)
        {
            Console.WriteLine("No bitcoin priced that high have been found, wait it will get there.");
        }
    }

    static List<Order> HandleSellOrder(Order sellOrder, List<OrderItem> bids, decimal currantBalance = 0.0m)
    {
        var usedAsks = new List<Order>();
        var availableBids = bids.Where(x => x.Order.Price >= sellOrder.Price).ToArray();

        for (int i = 0; i < availableBids.Count(); i++)
        {
            if (currantBalance - availableBids[i].Order.Amount >= 0.0m)
            {
                currantBalance -= availableBids[i].Order.Amount;

                Console.WriteLine($"Sell to full order {availableBids[i].Order}");

                usedAsks.Add(availableBids[i].Order);
                if (currantBalance - availableBids[i].Order.Amount == 0)
                {
                    break;
                }
            }
            else if (currantBalance - availableBids[i].Order.Amount < 0)
            {
                Console.WriteLine($"Partial sell {currantBalance} to order {availableBids[i].Order}");
                usedAsks.Add(availableBids[i].Order);
                break;
            }
        }
        return usedAsks;
    }


    private static void Buy(OrderBook[] orderBooks, Order requestedOrder)
    {
        decimal bitcoinsWithinLimitOrder = 0.0m;
        int countOfOrders = 0;

        for (int i = 0; bitcoinsWithinLimitOrder <= requestedOrder.Amount && i < orderBooks.Count(); i++)
        {
            Console.WriteLine($"Processing order book from {orderBooks[i].AcqTime}");
            if (orderBooks[i].BestMinAskPrice <= requestedOrder.Price)
            {
                var ordersToFulfill = HandleBuyOrder(requestedOrder, orderBooks[i].Asks, bitcoinsWithinLimitOrder);
                countOfOrders += ordersToFulfill.Count;
                bitcoinsWithinLimitOrder += ordersToFulfill.Sum(x => x.Amount);
            }
        }

        if (countOfOrders == 0)
        {
            Console.WriteLine("No bitcoins on sale found at the time of processing.");
        }
    }

    static List<Order> HandleBuyOrder(Order buyOrder, List<OrderItem> asks, decimal currentSum = 0.0m)
    {
        var usedAsks = new List<Order>();
        var availableAsks = asks.Where(x => x.Order.Price <= buyOrder.Price).ToArray();

        for (int i = 0; i < availableAsks.Length; i++)
        {
            if (currentSum + availableAsks[i].Order.Amount <= buyOrder.Amount)
            {
                currentSum += availableAsks[i].Order.Amount;

                Console.WriteLine($"Buy full order {availableAsks[i].Order}");

                usedAsks.Add(availableAsks[i].Order);
                if (currentSum + availableAsks[i].Order.Amount == buyOrder.Amount)
                {
                    break;
                }
            }
            else if (currentSum + availableAsks[i].Order.Amount > buyOrder.Amount)
            {
                Console.WriteLine($"Partial buy {buyOrder.Amount - currentSum} order {availableAsks[i].Order}");
                usedAsks.Add(availableAsks[i].Order);

                break;
            }
        }
        return usedAsks;
    }
}

