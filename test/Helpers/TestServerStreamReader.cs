using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace test.Helpers
{
    public class TestServerStreamReader<T> : IAsyncStreamReader<T> where T : class
    {
        private readonly Channel<T> _channel;
        private readonly ServerCallContext _serverCallContext;

        public T Current { get; private set; } = null!;

        public TestServerStreamReader(ServerCallContext serverCallContext)
        {
            _channel = Channel.CreateUnbounded<T>();
            _serverCallContext = serverCallContext;
        }

        public void AddMessage(T message)
        {
            if (!_channel.Writer.TryWrite(message))
            {
                throw new InvalidOperationException("Unable to write message.");
            }
        }

        public void Complete()
        {
            _channel.Writer.Complete();
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            _serverCallContext.CancellationToken.ThrowIfCancellationRequested();

            if (await _channel.Reader.WaitToReadAsync(cancellationToken) &&
              _channel.Reader.TryRead(out var message))
            {
                Current = message;
                return true;
            }
            else
            {
                Current = null!;
                return false;
            }
        }
    }
}