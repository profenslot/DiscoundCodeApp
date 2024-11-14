namespace Common.Services.Interfaces;

public interface IDiscountCodeService
{
    Task<List<string>> GenerateCodesAsync(byte length, ushort count, CancellationToken cancellationToken);
    Task<bool> IsCodeValidAsync(string discountCode, CancellationToken cancellationToken);
}
