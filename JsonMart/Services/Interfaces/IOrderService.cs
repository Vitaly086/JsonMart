using JsonMart.Dtos;

namespace JsonMart.Services.Interfaces;

public interface IOrderService
{
    Task<OperationResult> TryPayOrderAsync(int userId, int orderId, CancellationToken token);
    Task<IEnumerable<OrderDto>?> GetAllOrdersAsync(CancellationToken token);
    Task<OrderDto?> GetOrderAsync(int orderId, CancellationToken token);
    Task<OrderCreateResponseDto> CreateOrderAsync(OrderCreateDto orderCreateDto, CancellationToken token);
    Task<bool> DeleteOrderAsync(int orderId, CancellationToken token);
    Task<OrderUpdateResponseDto?> UpdateOrderAsync(int orderId, OrderUpdateDto orderUpdateDto, CancellationToken token);
    Task<List<int>> GetUnpaidOrdersOlderThanAsync(DateTime cutoffTime);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId, CancellationToken token);
}