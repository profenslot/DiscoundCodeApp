using Common.Services;
using Moq;
using StackExchange.Redis;

namespace UnitTest;

public class CodeGeneratorServiceUnitTests
{
    private readonly CodeGeneratorService _codeGeneratorService;
    private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
    private readonly Mock<IDatabase> _mockRedisDb;
    private readonly Mock<IBatch> _mockBatch;

    public CodeGeneratorServiceUnitTests()
    {
        _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
        _mockBatch = new Mock<IBatch>();
        _mockRedisDb = new Mock<IDatabase>();

        _mockConnectionMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockRedisDb.Object);

        _codeGeneratorService = new CodeGeneratorService(_mockConnectionMultiplexer.Object);
    }

    [Fact]
    public async Task GetCodesAsync_ShouldReturnUniqueCodes_WhenNoDuplicates()
    {
        // Arrange
        byte length = 8;
        ushort count = 5;

        _mockRedisDb
            .Setup(db => db.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisResult.Create(Array.Empty<RedisValue>(), ResultType.Array));

        // Act
        var result = await _codeGeneratorService.GetCodesAsync(length, count);

        // Assert
        Assert.True(result.Distinct().Count() == result.Count);
    }

    [Fact]
    public async Task GetCodesAsync_ShouldRetry_WhenSomeCodesAreNotUnique()
    {
        // Arrange
        byte length = 8;
        ushort count = 5;
        var sequence = new MockSequence();

        _mockRedisDb
            .InSequence(sequence)
            .Setup(db => db.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((string script, RedisKey[] keys, RedisValue[] values, CommandFlags flags) =>
            {
                // Simulate 2 codes being already present in Redis
                return RedisResult.Create(values.Take(2).ToArray(), ResultType.Array);
            });

        _mockRedisDb
            .InSequence(sequence)
            .Setup(db => db.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisResult.Create(Array.Empty<RedisValue>(), ResultType.Array));

        _mockRedisDb.Setup(r => r.CreateBatch(null)).Returns(_mockBatch.Object);

        // Act
        var result = await _codeGeneratorService.GetCodesAsync(length, count);

        // Assert
        Assert.Equal(count, result.Distinct().Count());
    }
}
