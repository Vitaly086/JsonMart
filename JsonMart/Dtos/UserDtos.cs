namespace JsonMart.Dtos;

public record UserDto(int Id, string Name, decimal Balance, List<int>? OrderIds);

public record UserCreateDto(string Name);

public record UserCreateResponseDto(int? Id = null, string? ErrorMessage = null);