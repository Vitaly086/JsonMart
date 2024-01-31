using JsonMart.Entities;

namespace JsonMart.Dtos;

public record OrderDto(int OrderId, DateTime OrderDate, int? UserId, List<OrderProductDto>? Products, OrderStatus Status, decimal TotalPrice);
public record OrderCreateDto(int UserId, List<int> ProductIds);
public record OrderProductDto(int ProductId, string? ProductName, decimal? Price, string? Description, int Quantity);
public record OrderUpdateDto(List<int> ProductIds);
public record OrderUpdateResponseDto(OperationResult Result, OrderDto? OrderDto);
public record OrderCreateResponseDto(
    OperationResult Result,
    int? OrderId = null, 
    DateTime? OrderDate = null, 
    int? UserId = null, 
    List<OrderProductDto>? Products = null, 
    OrderStatus Status = OrderStatus.Pending,
    decimal? TotalOrderPrice = null,
    List<ProductAvailabilityInfoDto>? UnavailableProducts = null
);