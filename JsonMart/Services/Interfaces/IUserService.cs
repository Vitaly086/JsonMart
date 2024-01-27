using JsonMart.Dtos;

namespace JsonMart.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken token);
    Task<UserCreateResponseDto?> CreateUserAsync(UserCreateDto userCreateDto, CancellationToken token);
}