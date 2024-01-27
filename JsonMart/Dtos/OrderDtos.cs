namespace JsonMart.Dtos;

public record OrderDto(int Id, DateTime OrderDate, string? CustomerName, List<OrderProductDto> Products, decimal TotalPrice);
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
    List<ProductAvailabilityInfoDto>? UnavailableProducts = null
);
