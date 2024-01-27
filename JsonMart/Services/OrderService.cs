using JsonMart.Context;
using JsonMart.Dtos;
using JsonMart.Entities;
using JsonMart.Extensions;
using JsonMart.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JsonMart.Services;

public class OrderService : IOrderService
{
    private readonly JsonMartDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly ILogger<OrderService> _logger;


    public OrderService(JsonMartDbContext dbContext, IUserService userService, ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _userService = userService;
        _logger = logger;
    }


    public async Task<IEnumerable<OrderDto>?> GetAllOrdersAsync(CancellationToken token)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .ToListAsync(token);

        return orders.Select(o => o.ToDto());
    }


    public async Task<OrderDto?> GetOrderAsync(int orderId, CancellationToken token)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, token);

        if (order == null)
        {
            return null;
        }

        return order.ToDto();
    }

    public async Task<OrderCreateResponseDto> CreateOrderAsync(OrderCreateDto orderCreateDto, CancellationToken token)
    {
        var user = await _userService.GetUserByIdAsync(orderCreateDto.UserId, token);
        if (user == null)
        {
            return new OrderCreateResponseDto(
                new OperationResult(false, "User not found.")
            );
        }

        var productQuantities = GroupProductQuantities(orderCreateDto);
        var productIds = productQuantities.Select(pq => pq.ProductId).ToList();

        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(token);

        var unavailableProducts = productQuantities
            .Select(pq => new
            {
                pq.ProductId, pq.Quantity, Product = products.FirstOrDefault(p => p.Id == pq.ProductId)
            })
            .Where(pq => pq.Product == null || pq.Product.AvailableQuantity < pq.Quantity)
            .Select(pq =>
                new ProductAvailabilityInfoDto(pq.ProductId, pq.Product?.Name, pq.Product?.AvailableQuantity ?? 0))
            .ToList();

        if (unavailableProducts.Any())
        {
            return new OrderCreateResponseDto(
                new OperationResult(false, "Some products are unavailable."),
                UnavailableProducts: unavailableProducts
            );
        }

        var newOrder = new OrderEntity
        {
            OrderDate = DateTime.UtcNow,
            CustomerName = user.Name,
            UserId = user.Id,
            OrderProducts = productQuantities
                .Select(pq => new OrderProduct
                {
                    ProductId = pq.ProductId,
                    ProductQuantity = pq.Quantity,
                    Product = products.First(p => p.Id == pq.ProductId)
                }).ToList()
        };

        foreach (var orderProduct in newOrder.OrderProducts)
        {
            orderProduct.Product.AvailableQuantity -= orderProduct.ProductQuantity;
        }

        await _dbContext.Orders.AddAsync(newOrder, token);
        await _dbContext.SaveChangesAsync(token);

        return new OrderCreateResponseDto(
            new OperationResult(true, "Order successfully created."),
            newOrder.Id,
            newOrder.OrderDate,
            newOrder.CustomerName,
            newOrder.OrderProducts.Select(op => new OrderProductDto(
                op.ProductId,
                op.Product.Name,
                op.Product.Price,
                op.Product.Description,
                op.ProductQuantity
            )).ToList(),
            newOrder.GetTotalOrderPrice(),
            new List<ProductAvailabilityInfoDto>()
        );
    }

    public async Task<OrderDto?> UpdateOrderAsync(int orderId, OrderUpdateDto orderUpdateDto, CancellationToken token)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(token);
        try
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderProducts)
                .FirstOrDefaultAsync(o => o.Id == orderId, token);

            if (order == null)
            {
                return null;
            }

            // Получаем ид продуктов для удаления
            var productIdsToRemove = order.OrderProducts
                .Select(op => op.ProductId)
                .Except(orderUpdateDto.ProductIds)
                .ToList();

            // Удаление продуктов, которых нет в новом списке
            foreach (var productId in productIdsToRemove)
            {
                var productToRemove = order.OrderProducts.FirstOrDefault(op => op.ProductId == productId);
                if (productToRemove != null)
                {
                    order.OrderProducts.Remove(productToRemove);
                }
            }

            // Получаем ид новых продуктов для добавления
            var newProductIds = orderUpdateDto.ProductIds
                .Except(order.OrderProducts.Select(op => op.ProductId))
                .ToList();

            // Проверяем наличие этих продуктов в базе данных
            var productsToAdd = await _dbContext.Products
                .Where(p => newProductIds.Contains(p.Id))
                .ToListAsync(token);

            // Добавление новых продуктов
            foreach (var product in productsToAdd)
            {
                order.OrderProducts.Add(new OrderProduct
                {
                    OrderId = orderId, ProductId = product.Id
                });
            }

            await _dbContext.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return order.ToDto();
        }

        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred while updating order with ID {orderId}");
            await transaction.RollbackAsync(token);
            throw;
        }
    }

    public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken token)
    {
        var order = await _dbContext.Orders
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, token);

        if (order == null)
        {
            return false;
        }
        
        foreach (var orderProduct in order.OrderProducts)
        {
            orderProduct.Product.AvailableQuantity += orderProduct.ProductQuantity;
        }

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(token);
        return true;
    }


    private List<(int ProductId, int Quantity)> GroupProductQuantities(OrderCreateDto orderCreateDto)
    {
        return orderCreateDto.ProductIds
            .GroupBy(productId => productId)
            .Select(group => (ProductId: group.Key, Quantity: group.Count()))
            .ToList();
    }
}