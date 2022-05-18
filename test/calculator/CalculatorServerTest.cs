using Xunit;
using Calculator;
using Calculator.Server.Services;
using test.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Grpc.Core;

namespace Test.calculator;

public class CalculatorServerTest
{
    [Fact]
    public async void Sum()
    {
        var service = new CalculatorServiceImpl();
        var response = await service.Sum(new SumRequest
        {
            FirstNumber = 1,
            SecondNumber = 1
        }, TestServerCallContext.Create());

        Assert.Equal(2, response.Result);
    }

    [Fact]
    public async void Primes()
    {
        var service = new CalculatorServiceImpl();
        var context = TestServerCallContext.Create();
        var stream = new TestServerStreamWriter<PrimeResponse>(context);

        using var call = service.Primes(new PrimeRequest
        {
            Number = 12390392840
        }, stream, context);

        await call;
        stream.Complete();

        var responses = new List<PrimeResponse>();
        await foreach (var response in stream.ReadAllAsync())
        {
            responses.Add(response);
        }

        Assert.True(responses.Count == 8);
        Assert.Equal(2, responses[0].Result);
        Assert.Equal(2, responses[1].Result);
        Assert.Equal(2, responses[2].Result);
        Assert.Equal(5, responses[3].Result);
        Assert.Equal(7, responses[4].Result);
        Assert.Equal(7, responses[5].Result);
        Assert.Equal(163, responses[6].Result);
        Assert.Equal(38783, responses[7].Result);
    }

    [Fact]
    public async void Avg()
    {
        var service = new CalculatorServiceImpl();
        var context = TestServerCallContext.Create();
        var stream = new TestServerStreamReader<AvgRequest>(context);

        using var call = service.Avg(stream, context);
        var numbers = Enumerable.Range(1, 10).ToList();

        foreach (var number in numbers)
        {
            stream.AddMessage(new AvgRequest { Number = number });
        }
        stream.Complete();

        var response = await call;

        Assert.Equal(5.5, response.Result);
    }

    [Fact]
    public async void Max()
    {
        var service = new CalculatorServiceImpl();
        var context = TestServerCallContext.Create();
        var responseStream = new TestServerStreamWriter<MaxResponse>(context);
        var requestStream = new TestServerStreamReader<MaxRequest>(context);

        using var call = service.Max(requestStream, responseStream, context);

        var numbers = new List<int>() { 4, 7, 2, 19, 4, 6, 32 };

        var responseReaderTask = Task.Run(async () =>
        {
            var expected = new List<int>() { 4, 7, 19, 32 };
            await foreach (var response in responseStream.ReadAllAsync())
            {
                Assert.Contains(response.Result, expected);
            }
        });

        foreach (var number in numbers)
        {
            requestStream.AddMessage(new MaxRequest
            {
                Number = number
            });
        }

        requestStream.Complete();
        await call;
        responseStream.Complete();
        await responseReaderTask;
    }

    [Fact]
    public async void Sqrt()
    {
        var service = new CalculatorServiceImpl();
        var context = TestServerCallContext.Create();

        var response = await service.Sqrt(new SqrtRequest
        {
            Number = 25
        }, context);

        Assert.Equal(5, response.Result);
    }

    [Fact]
    public async void SqrtInvalidArgument()
    {
        var service = new CalculatorServiceImpl();
        var context = TestServerCallContext.Create();

        var exception = await Assert.ThrowsAsync<RpcException>(async () => await service.Sqrt(new SqrtRequest
        {
            Number = -1
        }, context));

        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }
}