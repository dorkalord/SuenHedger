using System.Text.Json.Serialization;

public class OrderItemDto
{
    public OrderDTO Order { get; set; }
    public override string ToString()
    {
        return $"order {Order.Type} amount: {Order.Amount}, order price: {Order.Price}";
    }
}
public class OrderDTO
{
    public object Id { get; set; }
    public DateTime Time { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderTypeEnum Type { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderKindEnum Kind { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }

    public override string ToString()
    {
        return $"order amount: {Amount}, order price: {Price}";
    }
}

public class OrderBookDto
{
    public DateTime AcqTime { get; set; }
    /// <summary>
    /// Available buyers
    /// </summary>
    public List<OrderItemDto> Bids { get; set; }
    /// <summary>
    /// Available sellers
    /// </summary>
    public List<OrderItemDto> Asks { get; set; }

    public decimal BestMaxBidPrice { get; set; }
    public decimal BestMinAskPrice { get; set; }

    public override string ToString()
    {
        return $"Best buy price: {BestMaxBidPrice}, best sell price: {BestMinAskPrice}";
    }
}

