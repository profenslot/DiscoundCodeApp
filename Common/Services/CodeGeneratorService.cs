using Common.Services.Interfaces;
using StackExchange.Redis;

namespace Common.Services;

public class CodeGeneratorService : ICodeGeneratorService
{
    private const string _redisSetKey = "unique_set";
    private const string LuaInsertScript =
        """
            local failedItems = {}
            for _, item in ipairs(ARGV) do
                if redis.call('SADD', KEYS[1], item) == 0 then
                    table.insert(failedItems, item)
                end
            end
            return failedItems
        """;

    private readonly IDatabase _redisDb;

    public CodeGeneratorService(IConnectionMultiplexer iConnectionMultiplexer)
    {
        _redisDb = iConnectionMultiplexer.GetDatabase();
    }

    public async Task<List<string>> GetCodesAsync(byte length, ushort count)
    {
        var uniqueStrings = GenerateUniqueCodes(length, count);

        var notUniqueStrings = await InsertUniqueKeysAsync(uniqueStrings);

        while (notUniqueStrings.Length > 0)
        {
            var newUniqueStrings = GenerateUniqueCodes(length, notUniqueStrings.Length);

            uniqueStrings = uniqueStrings.Except(notUniqueStrings).ToList();
            uniqueStrings.AddRange(newUniqueStrings);

            notUniqueStrings = await InsertUniqueKeysAsync(newUniqueStrings);
        }

        return uniqueStrings;
    }

    private async Task<string[]> InsertUniqueKeysAsync(List<string> uniqueStrings)
    {
        var result = await _redisDb.ScriptEvaluateAsync(LuaInsertScript, [_redisSetKey], uniqueStrings.Select(x => new RedisValue(x)).ToArray());

        return (string[]?)result ?? Array.Empty<string>();
    }

    private static List<string> GenerateUniqueCodes(byte length, int count)
    {
        var result = new List<string>();

        for (int i = 0; i < count; i++)
        {
            Guid guid = Guid.NewGuid();

            string hex = BitConverter.ToString(guid.ToByteArray()).Replace("-", string.Empty);

            result.Add(hex.TrimStart('0')[..length].ToUpper());
        }

        return result;
    }
}
