namespace Common.Storage.Repositories.Interfaces;

public interface IDiscountCodeRepository
{
    Task<List<string>> GetDuplicatesAsync(List<string> codes, CancellationToken cancellationToken);
    Task InsertCodesAsync(List<string> codes, CancellationToken cancellationToken);
    Task<bool> IsCodeValidAsync(string discountCode, CancellationToken cancellationToken);
}
