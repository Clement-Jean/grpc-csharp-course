using Grpc.Core;

namespace Greet.Server.Services;

public class GreetServiceImpl : GreetService.GreetServiceBase
{
    private readonly int _awaitingTimeMs;

    public GreetServiceImpl(int awaitingTimeMs = 1000)
    {
        _awaitingTimeMs = awaitingTimeMs;
    }

    public override Task<GreetResponse> Greet(
        GreetRequest request,
        ServerCallContext ctx)
    {
        return Task.FromResult(new GreetResponse
        {
            Result = $"Hello {request.FirstName}"
        });
    }
    public override async Task GreetManyTimes(
        GreetRequest request,
        IServerStreamWriter<GreetResponse> responseStream,
        ServerCallContext context)
    {
        for (var i = 0; i < 10; ++i)
        {
            await responseStream.WriteAsync(new GreetResponse
            {
                Result = $"Hello {request.FirstName}"
            });
            await Task.Delay(_awaitingTimeMs);
        }
    }

    public override async Task<GreetResponse> LongGreet(
        IAsyncStreamReader<GreetRequest> requestStream,
        ServerCallContext context)
    {
        var names = new List<string>();

        await foreach (var request in requestStream.ReadAllAsync())
        {
            names.Add(request.FirstName);
        }

        return new GreetResponse
        {
            Result = string.Join("\n", names.ConvertAll(name => $"Hello {name}"))
        };
    }

    public override async Task GreetEveryone(
        IAsyncStreamReader<GreetRequest> requestStream,
        IServerStreamWriter<GreetResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new GreetResponse
            {
                Result = $"Hello {request.FirstName}"
            });
        }
    }

    public override async Task<GreetResponse> GreetWithDeadline(GreetRequest request, ServerCallContext context)
    {
        for (var i = 0; i < 3; ++i)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                throw new RpcException(new Status(StatusCode.Cancelled, ""));
            }

            await Task.Delay(_awaitingTimeMs);
        }

        return new GreetResponse { Result = $"Hello {request.FirstName}" };
    }
}