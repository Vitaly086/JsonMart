using JsonMart.Dtos;

namespace JsonMart.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken token);
    Task<List<UserDto>?> GetAllUsersAsync(CancellationToken token);
    Task<UserCreateResponseDto?> CreateUserAsync(UserCreateDto userCreateDto, CancellationToken token);
    Task<bool> TryIncreaseBalanceAsync(int userId, decimal amount, CancellationToken token);
    Task<bool> TryDecreaseBalanceAsync(int userId, decimal amount, CancellationToken token);
}