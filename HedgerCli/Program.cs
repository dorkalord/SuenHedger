using Microsoft.Extensions.Configuration;
using OrderDomain;

class Program
{
    static void Main()
    {
        var memConfiguration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
            ["OrderbookName"] = "order_books_data - origin"
        })
        .Build();

        var orderBookReaderService = new OrderBookReaderService(memConfiguration);
        var orderService = new OrderService(orderBookReaderService);

        var requestedOrder = ReadOrderViaCli();


        Console.WriteLine(orderService.ExecuteOrder(requestedOrder));

        var testBuyOrder = new RequestOrder()
        {
            Amount = 10,
            Price = 2966,
            Type = OrderTypeEnum.Buy,
            Kind = OrderKindEnum.Limit
        };
        Console.WriteLine(orderService.ExecuteOrder(testBuyOrder));

        var testSellOrder = new RequestOrder()
        {
            Amount = 10,
            Price = 2960,
            Type = OrderTypeEnum.Sell,
            Kind = OrderKindEnum.Limit
        };
        Console.WriteLine(orderService.ExecuteOrder(testSellOrder));
    }

    private static RequestOrder ReadOrderViaCli()
    {
        Console.WriteLine("Buy or sel BTC? ");

        OrderTypeEnum orderType = OrderTypeEnum.Buy;

        (int left, int top) = Console.GetCursorPosition();
        var decorator = "✅  ";
        ConsoleKeyInfo key;
        bool isSelected = false;

        while (!isSelected)
        {
            Console.SetCursorPosition(left, top);

            Console.WriteLine($"{(orderType == OrderTypeEnum.Buy ? decorator : "    ")}Buy ");
            Console.WriteLine($"{(orderType == OrderTypeEnum.Sell ? decorator : "    ")}Sell");

            key = Console.ReadKey(false);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    orderType = OrderTypeEnum.Buy;
                    break;

                case ConsoleKey.DownArrow:
                    orderType = OrderTypeEnum.Sell;
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
            case OrderTypeEnum.Buy:
                Console.WriteLine($"What is he maximum you are willing to pay per one BTC : ");
                break;
            case OrderTypeEnum.Sell:
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

        return new RequestOrder()
        {
            Amount = amount,
            Price = limit,
            Type = orderType,
            Kind = OrderKindEnum.Limit
        };
    }

}

