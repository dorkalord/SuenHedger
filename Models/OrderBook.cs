public class OrderBook
{
    public DateTime AcqTime { get; set; }
    /// <summary>
    /// Available to buyers
    /// </summary>
    public List<OrderItem> Bids { get; set; }
    /// <summary>
    /// Available to sellers
    /// </summary>
    public List<OrderItem> Asks { get; set; }

    public decimal BestMaxBidPrice { get; set; }
    public decimal BestMinAskPrice { get; set; }

    public override string ToString()
    {
        return $"Best buy price: {BestMaxBidPrice}, best sell price: {BestMinAskPrice}";
    }
}

