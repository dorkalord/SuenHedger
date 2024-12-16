using Microsoft.Extensions.DependencyInjection;

namespace OrderDomain
{
    public static class OrderDomainServiceCollectionExtension
    {
        public static IServiceCollection AddOrderDomainGroup(
             this IServiceCollection services)
        {
            services.AddScoped<IOrderBookReaderService, OrderBookReaderService>();
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
