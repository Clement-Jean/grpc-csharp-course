using Grpc.Net.Client;
using Grpc.Core;

using var channel = GrpcChannel.ForAddress("http://localhost:50051");