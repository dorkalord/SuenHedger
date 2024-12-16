using System.Text.Json.Serialization;

public class RequestOrder
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderTypeEnum Type;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderKindEnum Kind;
    public decimal Amount;
    public decimal Price;

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
