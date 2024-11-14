using Server_PreGenerate.Services.Interfaces;
using StackExchange.Redis;

namespace Server_PreGenerate.Services;

public class QueueService : IQueueService
{
    private readonly IDatabase _redisDb;

    public QueueService(IConnectionMultiplexer iConnectionMultiplexer)
    {
        _redisDb = iConnectionMultiplexer.GetDatabase();
    }

    public async Task<long> CheckQueueLengthAsync(string key) =>
        await _redisDb.ListLengthAsync(key);

    public async Task<long> AddToQueueAsync(string key, List<string> uniqueStrings) =>
        await _redisDb.ListRightPushAsync(key, uniqueStrings.Select(x => new RedisValue(x)).ToArray());

    public async Task<List<string>> GetUniqueStringsAsync(byte length, ushort count, CancellationToken cancellationToken)
    {
        var key = length == 7 ? RedisKeyConstants.LengthOfSevenQueueKey : RedisKeyConstants.LengthOfEightQueueKey;
        var uniqueStrings = new List<string>();

        while (uniqueStrings.Count != count)
        {
            var result = await _redisDb.ListLeftPopAsync(key, count - uniqueStrings.Count);

            if (result is not null)
            {
                uniqueStrings.AddRange(result.Select(x => x.ToString()));
            }

            if (uniqueStrings.Count != count)
            {
                await Task.Delay(500, cancellationToken);
            }
        }

        return uniqueStrings;
    }
}
