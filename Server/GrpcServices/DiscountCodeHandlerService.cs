using Common.Services.Interfaces;
using Common.Validators;
using DiscountCodeApp;
using Grpc.Core;
using Server.Services.Interfaces;

namespace Server.GrpcServices;

public class DiscountCodeHandlerService : DiscountCodeHandler.DiscountCodeHandlerBase
{
    private readonly IDiscountCodeService _discountCodeService;
    private readonly ILogger<DiscountCodeHandlerService> _logger;
    private readonly IQueueService _queueService;

    public DiscountCodeHandlerService(IDiscountCodeService discountCodeService, ILogger<DiscountCodeHandlerService> logger, IQueueService queueService)
    {
        _discountCodeService = discountCodeService;
        _logger = logger;
        _queueService = queueService;
    }

    public override async Task<GenerateReply> GenerateCode(GenerateRequest request, ServerCallContext context)
    {
        try
        {
            DiscountCodeValidator.DiscountCodeGenerateLengthCheck(request.Length);
            DiscountCodeValidator.DiscountCodeCountCheck(request.Count);

            var result = await _queueService.EnqueueAsync((byte)request.Length, (ushort)request.Count, context.CancellationToken);

            return new GenerateReply
            {
                Result = result
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
