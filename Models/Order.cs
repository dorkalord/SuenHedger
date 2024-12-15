using System.Text.Json.Serialization;

public enum OrderType { Buy, Sell };
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

