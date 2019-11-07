using Grpc.Core;
using Max;
using System;
using System.IO;
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

            var client = new FindMaxService.FindMaxServiceClient(channel);
            var stream = client.findMaximum();

            var responseReaderTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext())
                    Console.WriteLine(stream.ResponseStream.Current.Max);
            });

            int[] numbers = { 1, 5, 3, 6, 2, 20 };

            foreach (var number in numbers)
            {
                await stream.RequestStream.WriteAsync(new FindMaxRequest() { Number = number });
            }

            await stream.RequestStream.CompleteAsync();
            await responseReaderTask;

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
    }
}
