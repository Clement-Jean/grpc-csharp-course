#pragma warning disable

using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace test.Helpers
{
    public class TestServerStreamWriter<T> : IServerStreamWriter<T> where T : class
    {
        private readonly ServerCallContext _serverCallContext;
        private readonly Channel<T> _channel;

        public WriteOptions? WriteOptions { get; set; }

        public TestServerStreamWriter(ServerCallContext serverCallContext)
        {
            _channel = Channel.CreateUnbounded<T>();

            _serverCallContext = serverCallContext;
        }

        public void Complete()
        {
            _channel.Writer.Complete();
        }

        public IAsyncEnumerable<T> ReadAllAsync()
        {
            return _channel.Reader.ReadAllAsync();
        }

        public async Task<T?> ReadNextAsync()
        {
            if (await _channel.Reader.WaitToReadAsync())
            {
                _channel.Reader.TryRead(out var message);
                return message;
            }
            else
            {
                return null;
            }
        }

        public Task WriteAsync(T message)
        {
            return _serverCallContext.CancellationToken.IsCancellationRequested ||
                    _channel.Writer.TryWrite(message) == false ?
                throw new InvalidOperationException("Unable to write message.") :
                Task.CompletedTask;
        }
    }
}
#pragma warning restore
