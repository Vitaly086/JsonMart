using JsonMart.Entities;

namespace JsonMart.Dtos;

public record UserDto(int Id, string Name, List<OrderEntity> Orders);
public record UserCreateDto(string Name);
public record UserCreateResponseDto(int? Id = null, string? Name = null, string? ErrorMessage = null);
