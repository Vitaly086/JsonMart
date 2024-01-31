using JsonMart.Services.Interfaces;

namespace JsonMart.Services;

public class UnpaidOrdersCleanupService : BackgroundService
{
    private const int CHECK_INTERVAL_TIME = 3;
    private const int UNPAID_ORDER_LIFE_TIME = 20;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UnpaidOrdersCleanupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(CHECK_INTERVAL_TIME);
    private readonly TimeSpan _orderLifetime = TimeSpan.FromMinutes(UNPAID_ORDER_LIFE_TIME);

    
    public UnpaidOrdersCleanupService(IServiceProvider serviceProvider, ILogger<UnpaidOrdersCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Unpaid Orders Cleanup Service is running.");

            await DeleteUnpaidOrders(stoppingToken);

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task DeleteUnpaidOrders(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        var cutoffTime = DateTime.UtcNow - _orderLifetime;
        var orderIdsToDelete = await orderService.GetUnpaidOrdersOlderThanAsync(cutoffTime);

        foreach (var orderId in orderIdsToDelete)
        {
            await orderService.DeleteOrderAsync(orderId, token);
            _logger.LogInformation($"Deleted unpaid order with ID {orderId}");
        }

        if (orderIdsToDelete.Any())
        {
            _logger.LogInformation($"{orderIdsToDelete.Count} unpaid orders were deleted.");
        }
    }
}