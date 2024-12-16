
namespace OrderDomain
{
    public interface IOrderService
    {
        OrderResponse ExecuteOrder(RequestOrder order);
    }
}