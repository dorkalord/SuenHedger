using System.Text.Json.Serialization;

public class BookOrder
{
    public string ExchangeId { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
}

public class RequestBookOrder : BookOrder
{
    public decimal UseAmount { get; set; }

    public override string ToString()
    {
        return $"Execute order on exchange  {ExchangeId}, {Amount} @ {Price} take {UseAmount}\r\n";
    }
}

public class RequestOrder
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderTypeEnum Type { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderKindEnum Kind { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }

}

public class OrderResponse : RequestOrder
{
    public List<RequestBookOrder> requestBookOrders { get; set; }

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
