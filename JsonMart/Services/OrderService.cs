using JsonMart.Context;
using JsonMart.Dtos.Order;
using JsonMart.Dtos.Product;
using JsonMart.Entities;
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

        return orders.Select(ConvertToOrderDto);
    }


    public async Task<OrderDto?> GetOrderAsync(int orderId, CancellationToken token)
    {
        var orderEntity = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, token);

        if (orderEntity == null)
        {
            return null;
        }

        var orderDto = ConvertToOrderDto(orderEntity);
        return orderDto;
    }

    public async Task<OrderCreateResponseDto?> CreateOrderAsync(OrderCreateDto orderCreateDto, CancellationToken token)
    {
        var user = await _userService.GetUserByIdAsync(orderCreateDto.UserId, token);
        if (user == null)
        {
            return null;
        }

        var productQuantities = orderCreateDto.ProductIds
            .GroupBy(productId => productId)
            .Select(group => (ProductId: group.Key, Quantity: group.Count()))
            .ToList();

        var productIds = productQuantities.Select(pq => pq.ProductId).ToList();
        var stocks = await _dbContext.Stocks
            .Where(s => productIds.Contains(s.ProductId))
            .ToListAsync(token);

        var newOrderEntity = new OrderEntity()
        {
            OrderDate = DateTime.UtcNow,
            CustomerName = user.Name,
            UserId = orderCreateDto.UserId,
            OrderProducts = new List<OrderProduct>()
        };

        var unavailableProducts = new List<ProductAvailabilityInfoDto>();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(token);
        try
        {
            foreach (var productQuantity in productQuantities)
            {
                var stock = stocks.FirstOrDefault(s => s.ProductId == productQuantity.ProductId);

                if (stock == null || stock.AvailableQuantity < productQuantity.Quantity)
                {
                    unavailableProducts.Add(new ProductAvailabilityInfoDto
                    {
                        ProductId = productQuantity.ProductId,
                        ProductName = stock?.Product.Name,
                        AvailableQuantity = stock?.AvailableQuantity ?? 0
                    });
                    continue;
                }

                // Добавление продуктов в заказ
                for (var i = 0; i < productQuantity.Quantity; i++)
                {
                    newOrderEntity.OrderProducts.Add(new OrderProduct()
                    {
                        Order = newOrderEntity,
                        ProductId = productQuantity.ProductId,
                    });
                    stock.AvailableQuantity--;
                }
            }

            if (!newOrderEntity.OrderProducts.Any())
            {
                return new OrderCreateResponseDto
                {
                    UnavailableProducts = unavailableProducts
                };
            }

            await _dbContext.Orders.AddAsync(newOrderEntity, token);
            await _dbContext.SaveChangesAsync(token);
            await transaction.CommitAsync(token);

            var totalPrice = newOrderEntity.OrderProducts.Sum(op => op.Product.Price);
            var response = new OrderCreateResponseDto
            {
                Id = newOrderEntity.Id,
                OrderDate = newOrderEntity.OrderDate,
                CustomerName = newOrderEntity.CustomerName,
                Products = newOrderEntity.OrderProducts.Select(op => new OrderProductDto
                {
                    Id = op.ProductId,
                    Name = op.Product.Name,
                    Price = op.Product.Price,
                    Description = op.Product.Description,
                }).ToList(),
                TotalPrice = totalPrice ?? 0,
                UnavailableProducts = unavailableProducts
            };
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while creating order for user ID {orderCreateDto.UserId}");
            await transaction.RollbackAsync(token);
            return null;
        }
    }

    public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken token)
    {
        var order = await _dbContext.Orders.FindAsync(orderId, token);

        if (order == null)
        {
            return false;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(token);
        try
        {
            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred while deleting order with ID {orderId}");
            await transaction.RollbackAsync(token);
            return false;
        }
    }

    public async Task<bool> TryUpdateOrderAsync(int orderId, OrderUpdateDto orderUpdateDto, CancellationToken token)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(token);
        try
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderProducts)
                .FirstOrDefaultAsync(o => o.Id == orderId, token);

            if (order == null)
            {
                return false;
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
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred while updating order with ID {orderId}");
            await transaction.RollbackAsync(token);
            return false;
        }
    }

    private OrderDto ConvertToOrderDto(OrderEntity orderEntity)
    {
        return new OrderDto
        {
            Id = orderEntity.Id,
            OrderDate = orderEntity.OrderDate,
            CustomerName = orderEntity.CustomerName,
            Products = orderEntity.OrderProducts.Select(op => new OrderProductDto
            {
                Id = op.ProductId,
                Name = op.Product.Name,
                Price = op.Product.Price,
                Description = op.Product.Description,
            }).ToList(),
            TotalPrice = orderEntity.OrderProducts.Sum(op => op.Product.Price ?? 0)
        };
    }
}