using JsonMart.Dtos;

namespace JsonMart.Services.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>?> GetAllOrdersAsync(CancellationToken token);
    Task<OrderDto?> GetOrderAsync(int orderId, CancellationToken token);
    Task<OrderCreateResponseDto> CreateOrderAsync(OrderCreateDto orderCreateDto, CancellationToken token);
    Task<bool> DeleteOrderAsync(int orderId, CancellationToken token);
    Task<OrderDto?> UpdateOrderAsync(int orderId, OrderUpdateDto orderUpdateDto, CancellationToken token);
}