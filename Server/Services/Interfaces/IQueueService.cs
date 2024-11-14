namespace Server.Services.Interfaces;

public interface IQueueService
{
    Task<bool> EnqueueAsync(byte length, ushort count, CancellationToken cancellationToken);
}
