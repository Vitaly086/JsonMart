using JsonMart.Dtos.Order;

namespace JsonMart.Services.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>?> GetAllOrdersAsync(CancellationToken token);
    Task<OrderDto?> GetOrderAsync(int orderId, CancellationToken token);
    Task<OrderCreateResponseDto?> CreateOrderAsync(OrderCreateDto orderCreateDto, CancellationToken token);
    Task<bool> DeleteOrderAsync(int orderId, CancellationToken token);
    Task<bool> TryUpdateOrderAsync(int orderId, OrderUpdateDto orderUpdateDto, CancellationToken token);
}