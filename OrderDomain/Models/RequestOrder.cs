using System.Text.Json.Serialization;

public class BookOrder
{
    public DateTime ExchangeId { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
}

public class RequestBookOrder : BookOrder
{
    public decimal UseAmount { get; set; }

    public override string ToString()
    {
        return $"Execute order on exchange {ExchangeId.ToLongTimeString() + ":" +ExchangeId.Millisecond }, {Amount} BTC @ {Price} € take {UseAmount}\r\n";
    }
}

public class RequestOrder
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderTypeEnum Type { get; set; }
    public decimal Amount { get; set; }
}

public class OrderResponse : RequestOrder
{
    public List<RequestBookOrder> requestBookOrders { get; set; }
    public decimal totalCost => requestBookOrders.Sum(x => x.UseAmount * x.Price);
    public OrderResponse(RequestOrder requestOrder)
    {
        Type = requestOrder.Type;
        Amount = requestOrder.Amount;
        requestBookOrders = [];
    }

    public override string ToString()
    {
        return $"To fulfil request order of type {Type} for {Amount} BTC "
            + (Type == OrderTypeEnum.Buy ? $"you need {decimal.Round(totalCost, 2, MidpointRounding.AwayFromZero)} € " : $"you will get {decimal.Round(totalCost, 2, MidpointRounding.AwayFromZero)} € ")
            + $"execute orders:\r\n{string.Concat(requestBookOrders.Select(x => x.ToString()))}";
    }
}
