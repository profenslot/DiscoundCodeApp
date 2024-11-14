namespace Server_PreGenerate.Services.Interfaces;

public interface IQueueService
{
    Task<long> AddToQueueAsync(string key, List<string> uniqueStrings);
    Task<long> CheckQueueLengthAsync(string key);
    Task<List<string>> GetUniqueStringsAsync(byte length, ushort count, CancellationToken cancellationToken);
}
