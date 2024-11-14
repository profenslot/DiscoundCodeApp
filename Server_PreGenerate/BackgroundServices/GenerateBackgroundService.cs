using Common.Services.Interfaces;
using Server_PreGenerate.Services.Interfaces;

namespace Server_PreGenerate.BackgroundServices;

public class GenerateBackgroundService : BackgroundService
{
    private const int _batchSize = 50000;

    private readonly ILogger<GenerateBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public GenerateBackgroundService(
        ILogger<GenerateBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var task1 = Task.Run(() => GenerateDiscountCodesAsync(RedisKeyConstants.LengthOfSevenQueueKey, 7, stoppingToken), stoppingToken);
        var task2 = Task.Run(() => GenerateDiscountCodesAsync(RedisKeyConstants.LengthOfEightQueueKey, 8, stoppingToken), stoppingToken);

        await Task.WhenAll(task1, task2);
    }

    private async Task GenerateDiscountCodesAsync(string key, byte length, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var preService = scope.ServiceProvider.GetRequiredService<IQueueService>();

            var queueLength = await preService.CheckQueueLengthAsync(key);

            if (queueLength < _batchSize)
            {
                var _discountCodeService = scope.ServiceProvider.GetRequiredService<IDiscountCodeService>();

                var newStrings = await _discountCodeService.GenerateCodesAsync(length, (ushort)(_batchSize - queueLength), cancellationToken);
                _ = await preService.AddToQueueAsync(key, newStrings);

                _logger.LogInformation("Generated and added a new batch of {Count} unique strings with length of {Length}", newStrings.Count, length);
            }

            await Task.Delay(500, cancellationToken);
        }
    }
}
