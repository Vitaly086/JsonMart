using JsonMart.Context;
using JsonMart.Dtos.User;
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
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Id == userId, token);
        
        if (user == null)
        {
            return null;
        }

        return new UserDto()
        {
            Id = user.Id,
            Name = user.Name,
            Orders = user.Orders.ToList()
        };
    }

    public async Task<UserCreateResponseDto?> CreateUserAsync(UserCreateDto userCreateDto, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(userCreateDto.Name))
        {
            return new UserCreateResponseDto { ErrorMessage = "User name cannot be empty." };
        }

        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.Name == userCreateDto.Name, token);
        if (existingUser)
        {
            return new UserCreateResponseDto { ErrorMessage = "User name is already taken." };
        }

        var newUser = new UserEntity
        {
            Name = userCreateDto.Name
        };

        try
        {
            await _dbContext.Users.AddAsync(newUser, token);
            await _dbContext.SaveChangesAsync(token);

            return new UserCreateResponseDto
            {
                Id = newUser.Id,
                Name = newUser.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }
}