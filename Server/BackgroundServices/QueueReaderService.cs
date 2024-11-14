using Common.Services.Interfaces;
using Server.Services;
using System.Threading.Channels;

namespace Server.BackgroundServices;

public class QueueReaderService : BackgroundService
{
    private readonly Channel<QueueModel> _queue;
    private readonly IServiceProvider _serviceProvider;

    public QueueReaderService(Channel<QueueModel> queue, IServiceProvider serviceProvider)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var _discountCodeService = scope.ServiceProvider.GetRequiredService<IDiscountCodeService>();

            var result = await _discountCodeService.GenerateCodesAsync(item.Length, item.Count, stoppingToken);

            item.CompletionSource.SetResult(result.Count == item.Count);
        }
    }
}
