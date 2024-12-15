public class OrderItem
{
    public Order Order { get; set; }
    public override string ToString()
    {
        return $"order {Order.Type} amount: {Order.Amount}, order price: {Order.Price}";
    }
}

