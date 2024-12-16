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

public class BookOrder
{
    public DateTime ExchangeId;
    public decimal Amount;
    public decimal Price;
}

public class RequestBookOrder : BookOrder
{
    public decimal UseAmount;

    public override string ToString()
    {
        return $"Execute order on exchange  {ExchangeId}, {Amount} @ {Price} take {UseAmount}\r\n";
    }
}




public class RequestOrder
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderTypeEnum Type;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderKindEnum Kind;
    public decimal Amount;
    public decimal Price;

}

public class OrderResponse : RequestOrder
{
    public List<RequestBookOrder> requestBookOrders;
    
    public OrderResponse(RequestOrder requestOrder)
    {
        Type = requestOrder.Type;
        Kind = requestOrder.Kind;
        Amount = requestOrder.Amount;
        Price = requestOrder.Price;
        requestBookOrders = [];
    }

    public override string ToString()
    {
        return $"To fulfil request order {Type} of kind {Kind} for {Amount} @ {Price} \r\n{string.Concat(requestBookOrders.Select(x => x.ToString()))}";
    }
}