using Xunit;
using Greet;
using Greet.Server.Services;
using test.Helpers;
using System.Collections.Generic;
using System;
using System.Threading;
using Grpc.Core;

namespace Test.greet;

public class GreetServerTest
{
    [Fact]
    public async void Greet()
    {
        var service = new GreetServiceImpl();
        var response = await service.Greet(new GreetRequest
        {
            FirstName = "Clement"
        }, TestServerCallContext.Create());

        Assert.Equal("Hello Clement", response.Result);
    }

    [Fact]
    public async void GreetManyTimes()
    {
        var service = new GreetServiceImpl(awaitingTimeMs: 0);
        var context = TestServerCallContext.Create();
        var stream = new TestServerStreamWriter<GreetResponse>(context);

        using var call = service.GreetManyTimes(new GreetRequest
        {
            FirstName = "Clement"
        }, stream, context);

        await call;
        stream.Complete();

        var responses = new List<GreetResponse>();
        await foreach (var response in stream.ReadAllAsync())
        {
            responses.Add(response);
        }

        Assert.True(responses.Count == 10);
        foreach (var response in responses)
        {
            Assert.Equal("Hello Clement", response.Result);
        }
    }

    [Fact]
    public async void LongGreet()
    {
        var service = new GreetServiceImpl();
        var context = TestServerCallContext.Create();
        var stream = new TestServerStreamReader<GreetRequest>(context);

        using var call = service.LongGreet(stream, context);
        var names = new List<string>()
        {
            "Clement", "Marie", "Test"
        };

        foreach (var name in names)
        {
            stream.AddMessage(new GreetRequest { FirstName = name });
        }
        stream.Complete();

        var response = await call;

        Assert.Equal("Hello Clement\nHello Marie\nHello Test", response.Result);
    }

    [Fact]
    public async void GreetEveryone()
    {
        var service = new GreetServiceImpl();
        var context = TestServerCallContext.Create();
        var responseStream = new TestServerStreamWriter<GreetResponse>(context);
        var requestStream = new TestServerStreamReader<GreetRequest>(context);

        using var call = service.GreetEveryone(requestStream, responseStream, context);

        var names = new List<string>()
        {
            "Clement", "Marie", "Test"
        };

        foreach (var name in names)
        {
            requestStream.AddMessage(new GreetRequest
            {
                FirstName = name
            });
            Assert.Equal($"Hello {name}", (await responseStream.ReadNextAsync())!.Result);
        }

        requestStream.Complete();
        await call;
        responseStream.Complete();
        Assert.Null(await responseStream.ReadNextAsync());
    }

    [Fact]
    public async void GreetWithDeadline()
    {
        var service = new GreetServiceImpl(100);
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        var context = TestServerCallContext.Create(
            cancellationToken: cancellationTokenSource.Token
        );

        var response = await service.GreetWithDeadline(new GreetRequest
        {
            FirstName = "Clement"
        }, context);

        Assert.Equal("Hello Clement", response.Result);
    }

    [Fact]
    public async void GreetWithDeadlineExceeded()
    {
        var service = new GreetServiceImpl(100);
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var context = TestServerCallContext.Create(
            cancellationToken: cancellationTokenSource.Token
        );

        var exception = await Assert.ThrowsAsync<RpcException>(async () => await service.GreetWithDeadline(new GreetRequest
        {
            FirstName = "Clement"
        }, context));

        Assert.Equal(StatusCode.Cancelled, exception.Status.StatusCode);
    }
}