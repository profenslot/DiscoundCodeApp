using DiscoundCodeApp;
using Google.Rpc;
using Grpc.Core;
using Grpc.Net.Client;
using static DiscoundCodeApp.DiscountCodeHandler;

namespace Client;

internal static class Program
{
    static async Task Main()
    {
        //using var channel = GrpcChannel.ForAddress("https://localhost:7032"); // Server
        using var channel = GrpcChannel.ForAddress("https://localhost:7296"); // Server_PreGenerate
        //using var channel = GrpcChannel.ForAddress("https://localhost:7171"); // Server_Redis
        var client = new DiscountCodeHandlerClient(channel);

        while (true)
        {
            Console.WriteLine("Enter something (type 'exit' to quit): ");
            var userInput = Console.ReadLine();

            if (userInput?.ToLower() == "exit")
            {
                break;
            }

            try
            {
                var reply = await client.UseCodeAsync(new UseCodeRequest { Code = userInput });

                Console.WriteLine($"Response for {userInput}: {reply.Result}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Server error: {ex.Status.Detail}");

                var badRequest = ex.GetRpcStatus()?.GetDetail<BadRequest>();
                if (badRequest != null)
                {
                    foreach (var fieldViolation in badRequest.FieldViolations)
                    {
                        Console.WriteLine($"Field: {fieldViolation.Field}");
                        Console.WriteLine($"Description: {fieldViolation.Description}");
                    }
                }
            }
        }

        try
        {
            await SimulateConcurrentRequests(client, 200, 2000, 8);
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"Server error: {ex.Status.Detail}");

            var badRequest = ex.GetRpcStatus()?.GetDetail<BadRequest>();
            if (badRequest != null)
            {
                foreach (var fieldViolation in badRequest.FieldViolations)
                {
                    Console.WriteLine($"Field: {fieldViolation.Field}");
                    Console.WriteLine($"Description: {fieldViolation.Description}");
                }
            }
        }
    }

    static async Task SimulateConcurrentRequests(DiscountCodeHandlerClient client, int numberOfRequests, uint numberOfCodes, uint length)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < numberOfRequests; i++)
        {
            tasks.Add(SendRequestAsync(client, i, numberOfCodes, length));
        }

        await Task.WhenAll(tasks);

        Console.WriteLine("All requests completed.");
    }

    static async Task SendRequestAsync(DiscountCodeHandlerClient client, int requestId, uint numberOfCodes, uint length)
    {
        try
        {
            var reply = await client.GenerateCodeAsync(new GenerateRequest { Count = numberOfCodes, Length = length });

            Console.WriteLine($"Response for Request #{requestId}: {reply.Result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Request #{requestId}: {ex.Message}");
        }
    }
}
