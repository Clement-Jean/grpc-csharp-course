using Greeting;
using Grpc.Core;
using Grpc.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace server
{
    class Program
    {
        const int Port = 50051;

        static void Main(string[] args)
        {
            Server server = null;

            try
            {
                var keypair = new KeyCertificatePair(File.ReadAllText("ssl/server.crt"), File.ReadAllText("ssl/server.key"));
                var cacert = File.ReadAllText("ssl/ca.crt");
                var credentials = new SslServerCredentials(new List<KeyCertificatePair>() { keypair }, cacert, false);

                server = new Server()
                {
                    Services = { GreetingService.BindService(new GreetingServiceImpl()) },
                    Ports = { new ServerPort("localhost", Port, credentials) }
                };

                server.Start();
                Console.WriteLine("The server is listening on the port : " + Port);
                Console.ReadKey();
            }
            catch (IOException e)
            {
                Console.WriteLine("The server failed to start : " + e.Message);
                throw;
            }
            finally
            {
                if (server != null)
                    server.ShutdownAsync().Wait();
            }
        }
    }
}
