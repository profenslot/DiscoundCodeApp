using Common.Services.Interfaces;
using Common.Validators;
using DiscountCodeApp;
using Grpc.Core;

namespace Server_Redis.GrpcServices;

public class DiscountCodeHandlerService : DiscountCodeHandler.DiscountCodeHandlerBase
{
    private readonly IDiscountCodeService _discountCodeService;
    private readonly ILogger<DiscountCodeHandlerService> _logger;

    public DiscountCodeHandlerService(IDiscountCodeService discountCodeService, ILogger<DiscountCodeHandlerService> logger)
    {
        _discountCodeService = discountCodeService;
        _logger = logger;
    }

    public override async Task<GenerateReply> GenerateCode(GenerateRequest request, ServerCallContext context)
    {
        try
        {
            DiscountCodeValidator.DiscountCodeGenerateLengthCheck(request.Length);
            DiscountCodeValidator.DiscountCodeCountCheck(request.Count);

            var result = await _discountCodeService.GenerateCodesAsync(
                (byte)request.Length,
                (ushort)request.Count,
                context.CancellationToken);

            return new GenerateReply
            {
                Result = result.Count == request.Count,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating discount codes.");

            return new GenerateReply
            {
                Result = false
            };
        }
    }

    public override async Task<UseCodeReply> UseCode(UseCodeRequest request, ServerCallContext context)
    {
        DiscountCodeValidator.ArgumentNotNullOrEmpty(request.Code);
        DiscountCodeValidator.DiscountCodeLengthCheck(request.Code);

        var result = await _discountCodeService.IsCodeValidAsync(request.Code, context.CancellationToken);

        return new UseCodeReply
        {
            Result = result ? 1U : 0U
        };
    }
}
