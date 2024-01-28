using JsonMart.Entities;

namespace JsonMart.Dtos;

public record OrderDto(int Id, DateTime OrderDate, string? CustomerName, List<OrderProductDto>? Products, OrderStatus Status, decimal TotalPrice);
public record OrderCreateDto(int UserId, List<int> ProductIds);
public record OrderProductDto(int Id, string? Name, decimal? Price, string? Description, int Quantity);
public record OrderUpdateDto(List<int> ProductIds);
public record OrderCreateResponseDto(
    OperationResult Result,
    int? Id = null, 
    DateTime? OrderDate = null, 
    string? CustomerName = null, 
    List<OrderProductDto>? Products = null, 
    decimal? TotalOrderPrice = null,
    OrderStatus Status = OrderStatus.Pending,
    List<ProductAvailabilityInfoDto>? UnavailableProducts = null
);
