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

    public async Task<OperationResult> TryPayOrder(int userId, int orderId, CancellationToken token)
    {
        var order = await FindOrderWithProductAsync(orderId, token);

        if (order == null)
        {
            return new OperationResult(false, "Order not found.");
        }

        var validationError = ValidateOrder(userId, order);
        if (validationError != null)
        {
            return validationError;
        }

        var orderTotal = order.GetTotalOrderPrice();
        var deductionResult = await _userService.TryDecreaseBalanceAsync(order.UserId, orderTotal, token);

        if (!deductionResult)
        {
            return new OperationResult(false, "Insufficient balance to pay for the order.");
        }

        order.Status = OrderStatus.Paid;
        await _dbContext.SaveChangesAsync(token);
        return new OperationResult(true, "Order paid successfully.");
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
        var order = await FindOrderWithProductAsync(orderId, token, true);
        return order?.ToDto();
    }

    public async Task<OrderCreateResponseDto> CreateOrderAsync(OrderCreateDto orderCreateDto, CancellationToken token)
    {
        var user = await _userService.GetUserByIdAsync(orderCreateDto.UserId, token);
        if (user == null)
        {
            return CreateOrderCreationErrorResponse("User not found.");
        }

        var productQuantities = GroupProductQuantities(orderCreateDto);
        var productIds = productQuantities.Select(pq => pq.ProductId).ToList();

        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(token);

        var unavailableProducts = GetUnavailableProducts(productQuantities, products);

        if (unavailableProducts.Any())
        {
            return CreateOrderCreationErrorResponse("Some products are unavailable.", unavailableProducts);
        }

        var newOrder = CreateNewOrder(user, productQuantities, products);
        await _dbContext.Orders.AddAsync(newOrder, token);
        await _dbContext.SaveChangesAsync(token);

        return newOrder.ToCreateResponseDto(new OperationResult(true, "Order successfully created."));
    }

    public async Task<OrderUpdateResponseDto?> UpdateOrderAsync(int orderId, OrderUpdateDto orderUpdateDto,
        CancellationToken token)
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

            var missingProductIds = await GetMissingProductIdsAsync(orderUpdateDto.ProductIds, token);
            if (missingProductIds.Any())
            {
                var missingIdsString = String.Join(", ", missingProductIds);
                return new OrderUpdateResponseDto(
                    new OperationResult(false, $"Product IDs not found: {missingIdsString}"), null);
            }

            RemoveProductsFromOrder(order, orderUpdateDto, token);
            await AddNewProductsToOrder(order, orderUpdateDto, token);

            await _dbContext.SaveChangesAsync(token);
            await transaction.CommitAsync(token);

            var operationResult = new OperationResult(true, "Order updated successfully.");
            return order.ToUpdateResponseDto(operationResult);
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
        var order = await FindOrderWithProductAsync(orderId, token);

        if (order == null)
        {
            return false;
        }

        if (order.Status == OrderStatus.Paid)
        {
            var result = await RefundUserMoney(orderId, token, order);
            if (!result)
            {
                return false;
            }
        }

        foreach (var orderProduct in order.OrderProducts)
        {
            orderProduct.Product.AvailableQuantity += orderProduct.ProductQuantity;
        }

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(token);
        return true;
    }

    private async Task<bool> RefundUserMoney(int orderId, CancellationToken token, Order order)
    {
        var userId = await _dbContext.Orders
            .Where(o => o.Id == orderId)
            .Select(o => o.UserId)
            .FirstAsync(token);

        var totalPrice = order.OrderProducts.Sum(op => op.Product.Price * op.ProductQuantity);
        var balanceIncreased = await _userService.TryIncreaseBalanceAsync(userId, totalPrice, token);

        if (!balanceIncreased)
        {
            _logger.LogError(
                $"Failed to increase balance for user with ID {userId} after deleting order with ID {orderId}.");
            return false;
        }

        return true;
    }

    private List<(int ProductId, int Quantity)> GroupProductQuantities(OrderCreateDto orderCreateDto)
    {
        return orderCreateDto.ProductIds
            .GroupBy(productId => productId)
            .Select(group => (ProductId: group.Key, Quantity: group.Count()))
            .ToList();
    }

    private void RemoveProductsFromOrder(Order order, OrderUpdateDto orderUpdateDto, CancellationToken token)
    {
        var productIdsToRemove = order.OrderProducts
            .Select(op => op.ProductId)
            .Except(orderUpdateDto.ProductIds)
            .ToList();

        foreach (var productId in productIdsToRemove)
        {
            var productToRemove = order.OrderProducts.FirstOrDefault(op => op.ProductId == productId);
            if (productToRemove != null)
            {
                order.OrderProducts.Remove(productToRemove);
            }
        }
    }

    private async Task AddNewProductsToOrder(Order order, OrderUpdateDto orderUpdateDto, CancellationToken token)
    {
        var newProductIds = orderUpdateDto.ProductIds
            .Except(order.OrderProducts.Select(op => op.ProductId))
            .ToList();

        var productsToAdd = await _dbContext.Products
            .Where(p => newProductIds.Contains(p.Id))
            .ToListAsync(token);

        foreach (var product in productsToAdd)
        {
            order.OrderProducts.Add(new OrderProduct { OrderId = order.Id, ProductId = product.Id });
        }
    }

    private List<ProductAvailabilityInfoDto> GetUnavailableProducts(
        List<(int ProductId, int Quantity)> productQuantities, List<Product> products)
    {
        return productQuantities
            .Select(pq => new
            {
                pq.ProductId, pq.Quantity, Product = products.FirstOrDefault(p => p.Id == pq.ProductId)
            })
            .Where(pq => pq.Product == null || pq.Product.AvailableQuantity < pq.Quantity)
            .Select(pq =>
                new ProductAvailabilityInfoDto(pq.ProductId, pq.Product?.Name, pq.Product?.AvailableQuantity ?? 0))
            .ToList();
    }

    private OrderCreateResponseDto CreateOrderCreationErrorResponse(string message,
        List<ProductAvailabilityInfoDto> unavailableProducts = null)
    {
        return new OrderCreateResponseDto(new OperationResult(false, message),
            UnavailableProducts: unavailableProducts);
    }

    private Order CreateNewOrder(UserDto user, List<(int ProductId, int Quantity)> productQuantities,
        List<Product> products)
    {
        var newOrder = new Order
        {
            OrderDate = DateTime.UtcNow,
            UserId = user.Id,
            Status = OrderStatus.Pending,
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

        return newOrder;
    }

    private async Task<Order?> FindOrderWithProductAsync(int orderId, CancellationToken token,
        bool includeAsNoTracking = false)
    {
        var query = _dbContext.Orders.AsQueryable();

        if (includeAsNoTracking)
        {
            query.AsNoTracking();
        }

        var order = await query
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, token);

        return order;
    }

    private async Task<List<int>> GetMissingProductIdsAsync(List<int> productIds, CancellationToken token)
    {
        var existingProductIds = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(token);

        return productIds.Except(existingProductIds).ToList();
    }

    private OperationResult? ValidateOrder(int userId, Order order)
    {
        if (order.Status == OrderStatus.Paid)
        {
            return new OperationResult(false, "The order has already been paid for.");
        }

        if (order.UserId != userId)
        {
            return new OperationResult(false, "Access denied: You do not have permission to modify this order.");
        }

        return null;
    }
}