using Grpc.Core;

namespace Calculator.Server.Services;

public class CalculatorServiceImpl : CalculatorService.CalculatorServiceBase
{
    public override Task<SumResponse> Sum(
      SumRequest request,
      ServerCallContext context)
    {
        return Task.FromResult(new SumResponse
        {
            Result = request.FirstNumber + request.SecondNumber
        });
    }

    public override async Task Primes(
      PrimeRequest request,
      IServerStreamWriter<PrimeResponse> responseStream,
      ServerCallContext context)
    {
        long number = request.Number;
        int divisor = 2;

        while (number > 1)
        {
            if (number % divisor == 0)
            {
                number /= divisor;
                await responseStream.WriteAsync(new PrimeResponse()
                {
                    Result = divisor
                });
            }
            else
            {
                ++divisor;
            }
        }
    }

    public override async Task<AvgResponse> Avg(
        IAsyncStreamReader<AvgRequest> requestStream,
        ServerCallContext context)
    {
        var responses = new List<int>();

        await foreach (var request in requestStream.ReadAllAsync())
        {
            responses.Add(request.Number);
        }

        return new AvgResponse
        {
            Result = responses.Sum() / (float)responses.Count
        };
    }

    public override async Task Max(
        IAsyncStreamReader<MaxRequest> requestStream,
        IServerStreamWriter<MaxResponse> responseStream,
        ServerCallContext context)
    {
        int? max = null;

        await foreach (var request in requestStream.ReadAllAsync())
        {
            if (max == null || max < request.Number)
            {
                max = request.Number;
                await responseStream.WriteAsync(new MaxResponse
                {
                    Result = max.Value
                });
            }
        }
    }

    public override Task<SqrtResponse> Sqrt(
        SqrtRequest request,
        ServerCallContext context)
    {
        int number = request.Number;

        if (number < 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "number < 0"));
        }

        return Task.FromResult(new SqrtResponse
        {
            Result = Math.Sqrt(number)
        });
    }
}