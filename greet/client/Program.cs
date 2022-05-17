using GreetPb;
using Grpc.Net.Client;
using Grpc.Core;

static async Task DoGreet(GreetService.GreetServiceClient client)
{
    GreetResponse response = await client.GreetAsync(new GreetRequest
    {
        FirstName = "Clement"
    });
    Console.WriteLine($"Greet: {response.Result}");
}

static async Task DoGreetManyTimes(GreetService.GreetServiceClient client)
{
    using var call = client.GreetManyTimes(new GreetRequest
    {
        FirstName = "Clement"
    });

    await foreach (var response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"GreetManyTimes: {response.Result}");
    }
}

static async Task DoLongGreet(GreetService.GreetServiceClient client)
{
    using var call = client.LongGreet();
    var names = new List<string>() { "Clement", "Marie", "Test" };

    foreach (var name in names)
    {
        await call.RequestStream.WriteAsync(new GreetRequest
        {
            FirstName = name
        });
    }

    await call.RequestStream.CompleteAsync();
    var response = await call;

    Console.WriteLine($"LongGreet: {response.Result}");
}

static async Task DoGreetEveryone(GreetService.GreetServiceClient client)
{
    using var call = client.GreetEveryone();
    var names = new List<string>() { "Clement", "Marie", "Test" };

    var responseReaderTask = Task.Run(async () =>
    {
        while (await call.ResponseStream.MoveNext())
        {
            Console.WriteLine($"GreetEveryone: {call.ResponseStream.Current.Result}");
        }
    });

    foreach (var name in names)
    {
        await call.RequestStream.WriteAsync(new GreetRequest
        {
            FirstName = name
        });
    }

    await call.RequestStream.CompleteAsync();
    await responseReaderTask;
}

static async Task DoGreetWithDeadline(GreetService.GreetServiceClient client, DateTime deadline)
{
    try
    {
        var response = await client.GreetWithDeadlineAsync(
            new GreetRequest() { FirstName = "Clement" },
            deadline: deadline
        );

        Console.WriteLine($"GreetWithDeadline: {response.Result}");
    }
    catch (RpcException e) when (e.StatusCode == StatusCode.DeadlineExceeded)
    {
        Console.WriteLine("Error: " + e.Status.StatusCode);
    }
}

var tls = false; // change if needed
var protocol = tls ? "https" : "http";
using var channel = GrpcChannel.ForAddress($"{protocol}://localhost:50051");
var client = new GreetService.GreetServiceClient(channel);

await DoGreet(client);
// await DoGreetManyTimes(client);
// await DoLongGreet(client);
// await DoGreetEveryone(client);
// await DoGreetWithDeadline(client, DateTime.UtcNow.AddSeconds(1));
// await DoGreetWithDeadline(client, DateTime.UtcNow.AddSeconds(5));
