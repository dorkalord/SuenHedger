using Microsoft.AspNetCore.Mvc;
using OrderDomain;

namespace HedgehogAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public OrderResponse ExecuteOrder(RequestOrder order)
        {
            return _orderService.ExecuteOrder(order);
        }
    }
}
