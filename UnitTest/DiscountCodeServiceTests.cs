using Common.Services;
using Common.Services.Interfaces;
using Common.Storage.Repositories.Interfaces;
using Moq;

namespace UnitTest;

public class DiscountCodeServiceTests
{
    private readonly Mock<IDiscountCodeRepository> _mockRepository;
    private readonly Mock<ICodeGeneratorService> _mockCodeGeneratorService;
    private readonly DiscountCodeService _discountCodeService;

    public DiscountCodeServiceTests()
    {
        _mockRepository = new Mock<IDiscountCodeRepository>();
        _mockCodeGeneratorService = new Mock<ICodeGeneratorService>();
        _discountCodeService = new DiscountCodeService(_mockRepository.Object, _mockCodeGeneratorService.Object);
    }

    [Fact]
    public async Task IsCodeValidAsync_ShouldReturnTrue_WhenCodeIsValid()
    {
        // Arrange
        var discountCode = "VALIDCODE123";
        var cancellationToken = CancellationToken.None;
        _mockRepository.Setup(r => r.IsCodeValidAsync(discountCode, cancellationToken)).ReturnsAsync(true);

        // Act
        var result = await _discountCodeService.IsCodeValidAsync(discountCode, cancellationToken);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.IsCodeValidAsync(discountCode, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task IsCodeValidAsync_ShouldReturnFalse_WhenCodeIsNotValid()
    {
        // Arrange
        var discountCode = "INVALIDCODE123";
        var cancellationToken = CancellationToken.None;
        _mockRepository.Setup(r => r.IsCodeValidAsync(discountCode, cancellationToken)).ReturnsAsync(false);

        // Act
        var result = await _discountCodeService.IsCodeValidAsync(discountCode, cancellationToken);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.IsCodeValidAsync(discountCode, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GenerateCodesAsync_ShouldGenerateCodesAndInsert()
    {
        // Arrange
        byte length = 8;
        ushort count = 10;
        var cancellationToken = CancellationToken.None;

        var generatedCodes = new List<string> { "CODE1", "CODE2", "CODE3", "CODE4", "CODE5", "CODE6", "CODE7", "CODE8", "CODE9", "CODE10" };
        _mockCodeGeneratorService.Setup(c => c.GetCodesAsync(length, count)).ReturnsAsync(generatedCodes);
        _mockRepository.Setup(r => r.InsertCodesAsync(generatedCodes, cancellationToken)).Returns(Task.CompletedTask);

        // Act
        var result = await _discountCodeService.GenerateCodesAsync(length, count, cancellationToken);

        // Assert
        Assert.Equal(generatedCodes, result);
        _mockCodeGeneratorService.Verify(c => c.GetCodesAsync(length, count), Times.Once);
        _mockRepository.Verify(r => r.InsertCodesAsync(generatedCodes, cancellationToken), Times.Once);
    }
}
