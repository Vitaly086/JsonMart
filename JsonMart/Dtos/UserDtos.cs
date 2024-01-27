using JsonMart.Entities;

namespace JsonMart.Dtos;

public record UserDto(int Id, string Name, decimal Balance, List<OrderDto> Orders);

public record UserCreateDto(string Name);

public record UserCreateResponseDto(int? Id = null, string? Name = null, string? ErrorMessage = null);