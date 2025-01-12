﻿using Microsoft.Extensions.Configuration;
using OrderDomain;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        var memConfiguration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
            ["OrderbookName"] = "order_books_data - origin"
        })
        .Build();

        var orderBookReaderService = new OrderBookReaderService(memConfiguration);
        var orderService = new OrderService(orderBookReaderService);

        var requestedOrder = args.Count() != 2 ? ReadOrderViaCli() : ReadOrderViaArgs(args);
        Console.WriteLine(orderService.ExecuteOrder(requestedOrder));

        //var testBuyOrder = new RequestOrder()
        //{
        //    Amount = 2,
        //    Type = OrderTypeEnum.Buy,
        //};
        //Console.WriteLine(orderService.ExecuteOrder(testBuyOrder));

        //var testSellOrder = new RequestOrder()
        //{
        //    Amount = 2,
        //    Type = OrderTypeEnum.Sell,
        //};
        //Console.WriteLine(orderService.ExecuteOrder(testSellOrder));
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
        var amount = ValidateAmount(Console.ReadLine());

        switch (orderType)
        {
            case OrderTypeEnum.Buy:
                Console.WriteLine($"What is he maximum you are willing to pay per one BTC : ");
                break;
            case OrderTypeEnum.Sell:
                Console.WriteLine($"What is he minimum you are willing to sell per one BTC : ");
                break;
        }

        Console.WriteLine($"Order {orderType} for {amount} is now processing...");

        return new RequestOrder()
        {
            Amount = amount,
            Type = orderType,
        };
    }

    private static RequestOrder ReadOrderViaArgs(string[] args)
    {
        if (args.Length < 2)
        {
            throw new Exception("Missing arguments. Example of a valid arguments Buy 2,10");
        }
        if (args.Length > 2)
        {
            throw new Exception("Too many arguments. Example of a valid arguments Buy 2,10");
        }

        if (!Enum.TryParse(typeof(OrderTypeEnum), args[0], out var orderType))
        {
            throw new InvalidCastException("We could not parse the provided value. Use Buy or Sale");
        }

        return new RequestOrder()
        {
            Amount = ValidateAmount(args[1]),
            Type = (OrderTypeEnum)orderType,
        };
    }

    private static decimal ValidateAmount(string amountString)
    {
        var isValidDecimal = decimal.TryParse(amountString.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var amount);

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

        return amount;
    }
}