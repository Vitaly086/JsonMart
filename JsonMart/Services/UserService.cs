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
            .AsNoTracking()
            .Include(u => u.Orders)
            .ThenInclude(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(u => u.Id == userId, token);

        if (user == null)
        {
            return null;
        }

        var orderDtos = user.Orders.Select(o => new OrderDto
        (
            o.Id,
            o.OrderDate,
            o.CustomerName,
            o.OrderProducts.Select(op => new OrderProductDto
            (
                op.ProductId,
                op.Product.Name,
                op.Product.Price,
                op.Product.Description,
                op.ProductQuantity
            )).ToList(),
            o.OrderProducts.Sum(op => op.Product.Price * op.ProductQuantity)
        )).ToList();
        return new UserDto(user.Id, user.Name, orderDtos);
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

            return new UserCreateResponseDto(newUser.Id, newUser.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }
}