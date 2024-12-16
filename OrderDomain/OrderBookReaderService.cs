using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class OrderBookReaderService : IOrderBookReaderService
{
    private string _orderbookName;

    public OrderBookReaderService(IConfiguration configuration)
    {
        _orderbookName = configuration?["OrderbookName"] ?? "order_books_data - origin";
    }

    public OrderBookDto[] LoadOrderBooks()
    {
        var lines = File.ReadAllLines(_orderbookName);
        OrderBookDto[] orderBooks = new OrderBookDto[lines.Length];
        int i = 0;
        foreach (var line in lines)
        {
            if (line.Length > 1)
            {
                var jsonStart = line.IndexOf('{') - 1;
                var orderBook = JsonSerializer.Deserialize<OrderBookDto>(line[jsonStart..]);

                if (orderBook is null)
                {
                    throw new Exception("Problem reading order book.");
                }

                orderBooks[i] = orderBook;

            }
            i++;
        }

        return orderBooks;
    }
}