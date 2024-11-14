using Common.Services.Interfaces;
using Common.Storage.Repositories.Interfaces;

namespace Common.Services;

public class DiscountCodeService : IDiscountCodeService
{
    private readonly IDiscountCodeRepository _discountcodeRepository;
    private readonly ICodeGeneratorService _codeGeneratorService;

    public DiscountCodeService(IDiscountCodeRepository discountcodeRepository, ICodeGeneratorService codeGeneratorService)
    {
        _discountcodeRepository = discountcodeRepository;
        _codeGeneratorService = codeGeneratorService;
    }

    public async Task<bool> IsCodeValidAsync(string discountCode, CancellationToken cancellationToken)
    {
        return await _discountcodeRepository.IsCodeValidAsync(discountCode, cancellationToken);
    }

    public async Task<List<string>> GenerateCodesAsync(byte length, ushort count, CancellationToken cancellationToken)
    {
        var codes = await _codeGeneratorService.GetCodesAsync(length, count);
        await _discountcodeRepository.InsertCodesAsync(codes, cancellationToken);

        return codes;
    }
}
