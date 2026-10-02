using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NetLpr.Core.Datas;
using NetLpr.Core.Log;

namespace NetLpr.Core.Services.Syncronizations
{
    public class SynchronizationServiceContext : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SyncChannelContext> _streamContexts = new();
        private readonly CancellationTokenSource _globalCts = new();

        public SynchronizationServiceContext(ILogger logger)
        {
            _logger = logger;
        }

        public void Register(string id, Action<InferencedInput> handler)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token);
            var context = new SyncChannelContext(id, handler, _logger, cts);

            if (_streamContexts.TryAdd(id, context))
            {
               
                context.StartProcessing();
            }
            else
            {
                cts.Dispose(); 
            }
        }

        public void Unregister(string id)
        {
            if (_streamContexts.TryRemove(id, out var context))
            {
                context.Dispose(); 
            }
        }

        public void Enqueue(string id, InferencedInput data)
        {
            if (_streamContexts.TryGetValue(id, out var context))
            {
                context.Enqueue(data);
            }
        }

        public void Dispose()
        {
            _globalCts.Cancel();
            foreach (var context in _streamContexts.Values)
            {
                context.Dispose();
            }
            _streamContexts.Clear();
            _globalCts.Dispose();
        }

        private class SyncChannelContext : IDisposable
        {
            private readonly string _id;
            private readonly Action<InferencedInput> _handler;
            private readonly ILogger _logger;
            private readonly Channel<InferencedInput> _channel;
            private readonly CancellationTokenSource _cts;

            public SyncChannelContext(string id, Action<InferencedInput> handler, ILogger logger, CancellationTokenSource cts)
            {
                _id = id;
                _handler = handler;
                _logger = logger;
                _cts = cts;

                _channel = Channel.CreateUnbounded<InferencedInput>(new UnboundedChannelOptions
                {
                    SingleWriter = false,
                    SingleReader = true,
                    AllowSynchronousContinuations = false
                });
            }

            public void StartProcessing()
            {
                Task.Run(ProcessAsync, _cts.Token);
            }

            public void Enqueue(InferencedInput data)
            {
                _channel.Writer.TryWrite(data);
            }

            private async Task ProcessAsync()
            {
                try
                {
                    await foreach (var frame in _channel.Reader.ReadAllAsync(_cts.Token))
                    {
                        if (frame == null) continue;

                        while (!frame.IsProcessed)
                        {
                            await Task.Delay(5, _cts.Token);
                        }

                        try
                        {
                            _handler.Invoke(frame);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Synchronization Handler Error [{_id}]");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Channel processing failed for [{_id}]");
                }
            }

            public void Dispose()
            {
                _cts.Cancel();
                _channel.Writer.TryComplete(); 
                _cts.Dispose();
            }
        }
    }
}