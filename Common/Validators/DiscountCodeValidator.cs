using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using System.Runtime.CompilerServices;

namespace Common.Validators;

public static class DiscountCodeValidator
{
    public static void ArgumentNotNullOrEmpty(string value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            var status = new Google.Rpc.Status
            {
                Code = (int)Code.InvalidArgument,
                Message = "Bad request",
                Details =
                {
                    Any.Pack(new BadRequest
                    {
                        FieldViolations =
                        {
                            new BadRequest.Types.FieldViolation
                            {
                                Field = paramName,
                                Description = "Value is null or empty"
                            }
                        }
                    })
                }
            };
            throw status.ToRpcException();
        }
    }

    public static void DiscountCodeGenerateLengthCheck(uint value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value != 7 && value != 8)
        {
            var status = new Google.Rpc.Status
            {
                Code = (int)Code.InvalidArgument,
                Message = "Bad request",
                Details =
                {
                    Any.Pack(new BadRequest
                    {
                        FieldViolations =
                        {
                            new BadRequest.Types.FieldViolation
                            {
                                Field = paramName,
                                Description = "Discount code length can be only 7 or 8"
                            }
                        }
                    })
                }
            };
            throw status.ToRpcException();
        }
    }

    public static void DiscountCodeCountCheck(uint value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value > 2000)
        {
            var status = new Google.Rpc.Status
            {
                Code = (int)Code.InvalidArgument,
                Message = "Bad request",
                Details =
                {
                    Any.Pack(new BadRequest
                    {
                        FieldViolations =
                        {
                            new BadRequest.Types.FieldViolation
                            {
                                Field = paramName,
                                Description = "You can only generate 2000 codes at a time"
                            }
                        }
                    })
                }
            };
            throw status.ToRpcException();
        }
    }

    public static void DiscountCodeLengthCheck(string value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value.Length != 7 && value.Length != 8)
        {
            var status = new Google.Rpc.Status
            {
                Code = (int)Code.InvalidArgument,
                Message = "Bad request",
                Details =
                {
                    Any.Pack(new BadRequest
                    {
                        FieldViolations =
                        {
                            new BadRequest.Types.FieldViolation
                            {
                                Field = paramName,
                                Description = "Discount code length can be only 7 or 8"
                            }
                        }
                    })
                }
            };
            throw status.ToRpcException();
        }
    }
}
