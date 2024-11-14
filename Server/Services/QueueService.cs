using Server.Services.Interfaces;
using System.Threading.Channels;

namespace Server.Services;

public class QueueService : IQueueService
{
    private readonly Channel<QueueModel> _queue;

    public QueueService(Channel<QueueModel> queue)
    {
        _queue = queue;
    }

    public async Task<bool> EnqueueAsync(byte length, ushort count, CancellationToken cancellationToken)
    {
        var model = new QueueModel(length, count, new TaskCompletionSource<bool>());
        await _queue.Writer.WriteAsync(model, cancellationToken);

        return await model.CompletionSource.Task;
    }
}

public record QueueModel(byte Length, ushort Count, TaskCompletionSource<bool> CompletionSource);
