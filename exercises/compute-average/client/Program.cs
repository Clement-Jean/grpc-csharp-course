using Average;
using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        const string target = "127.0.0.1:50052";

        static async Task Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("The client connected successfully");
            });

            var client = new AverageService.AverageServiceClient(channel);
            var stream = client.ComputeAverage();

            foreach (int number in Enumerable.Range(1, 4))
            {
                var request = new AverageRequest() { Number = number };

                await stream.RequestStream.WriteAsync(request);
            }

            await stream.RequestStream.CompleteAsync();

            var response = await stream.ResponseAsync;

            Console.WriteLine(response.Result);
            
            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
    }
}
