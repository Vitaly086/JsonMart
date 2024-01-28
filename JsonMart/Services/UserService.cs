using JsonMart.Context;
using JsonMart.Dtos;
using JsonMart.Entities;
using JsonMart.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JsonMart.Services;

public class UserService : IUserService
{
    private readonly JsonMartDbContext _dbContext;
    private readonly ILogger<JsonMartDbContext> _logger;

    public UserService(JsonMartDbContext dbContext, ILogger<JsonMartDbContext> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken token)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.Orders)
            .Select(u => new UserDto
            (
                u.Id,
                u.Name,
                u.Balance,
                u.Orders.Select(o => o.Id).ToList()
            ))
            .FirstOrDefaultAsync(token);

        return user;
    }

    public async Task<List<UserDto>?> GetAllUsersAsync(CancellationToken token)
    {
        var users = await _dbContext.Users
            .Include(u => u.Orders)
            .Select(u => new UserDto
            (u.Id,
                u.Name,
                u.Balance,
                u.Orders.Select(o => o.Id).ToList()
            ))
            .ToListAsync(token);
        
        return users;
    }


    public async Task<UserCreateResponseDto?> CreateUserAsync(UserCreateDto userCreateDto, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(userCreateDto.Name))
        {
            return new UserCreateResponseDto(ErrorMessage: "User name cannot be empty.");
        }

        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.Name == userCreateDto.Name, token);
        if (existingUser)
        {
            return new UserCreateResponseDto(ErrorMessage: "User name is already taken.");
        }

        var newUser = new UserEntity
        {
            Name = userCreateDto.Name
        };

        try
        {
            await _dbContext.Users.AddAsync(newUser, token);
            await _dbContext.SaveChangesAsync(token);

            return new UserCreateResponseDto(newUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public async Task<bool> TryIncreaseBalanceAsync(int userId, decimal amount, CancellationToken token)
    {
        var user = await _dbContext.Users.FindAsync(userId, token);
        if (user == null)
        {
            return false;
        }

        user.Balance += amount;
        await _dbContext.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> TryDecreaseBalanceAsync(int userId, decimal amount, CancellationToken token)
    {
        var user = await _dbContext.Users.FindAsync(userId, token);
        if (user == null || user.Balance < amount)
        {
            return false;
        }

        user.Balance -= amount;
        await _dbContext.SaveChangesAsync(token);
        return true;
    }
}