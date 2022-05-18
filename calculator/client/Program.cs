using Calculator;
using Grpc.Net.Client;
using Grpc.Core;

static async Task DoSum(CalculatorService.CalculatorServiceClient client)
{
    SumResponse response = await client.SumAsync(new SumRequest
    {
        FirstNumber = 1,
        SecondNumber = 1
    });

    Console.WriteLine($"Sum: {response.Result}");
}

static async Task DoPrimes(CalculatorService.CalculatorServiceClient client)
{
    using var call = client.Primes(new PrimeRequest
    {
        Number = 12390392840
    });

    await foreach (var response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Primes: {response.Result}");
    }
}

static async Task DoAvg(CalculatorService.CalculatorServiceClient client)
{
    using var call = client.Avg();
    var numbers = Enumerable.Range(1, 10).ToList();

    foreach (var number in numbers)
    {
        Console.WriteLine(number);
        await call.RequestStream.WriteAsync(new AvgRequest
        {
            Number = number
        });
    }

    await call.RequestStream.CompleteAsync();
    var response = await call;

    Console.WriteLine($"Avg: {response.Result}");
}

static async Task DoMax(CalculatorService.CalculatorServiceClient client)
{
    using var call = client.Max();
    var numbers = new List<int>() { 4, 7, 2, 19, 4, 6, 32 };

    var responseReaderTask = Task.Run(async () =>
    {
        while (await call.ResponseStream.MoveNext())
        {
            Console.WriteLine($"Max: {call.ResponseStream.Current.Result}");
        }
    });

    foreach (var number in numbers)
    {
        await call.RequestStream.WriteAsync(new MaxRequest
        {
            Number = number
        });
    }

    await call.RequestStream.CompleteAsync();
    await responseReaderTask;
}

static async Task DoSqrt(CalculatorService.CalculatorServiceClient client, int number)
{
    try
    {
        var response = await client.SqrtAsync(new SqrtRequest()
        {
            Number = number
        });

        Console.WriteLine($"Sqrt: {response.Result}");
    }
    catch (RpcException e) when (e.StatusCode == StatusCode.InvalidArgument)
    {
        Console.WriteLine("Error: " + e.Status);
    }
}

using var channel = GrpcChannel.ForAddress("http://localhost:50051");
var client = new CalculatorService.CalculatorServiceClient(channel);


// await DoSum(client);
// await DoPrimes(client);
// await DoAvg(client);
// await DoMax(client);
// await DoSqrt(client, -1);
await DoSqrt(client, 25);