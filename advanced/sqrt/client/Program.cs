using Grpc.Core;
using Sqrt;
using System;
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

            var client = new SqrtService.SqrtServiceClient(channel);
            int number = 16;

            try
            {
                var response = client.sqrt(new SqrtRequest() { Number = number },
                                            deadline: DateTime.UtcNow.AddSeconds(1));

                Console.WriteLine(response.SquareRoot);
            }
            catch (RpcException e) when (e.StatusCode == StatusCode.InvalidArgument)
            {
                Console.WriteLine("Error : " + e.Status.Detail);
            }
            catch (RpcException e) when (e.StatusCode == StatusCode.DeadlineExceeded)
            {
                Console.WriteLine("Deadline exceeded !");
            }

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
    }
}
