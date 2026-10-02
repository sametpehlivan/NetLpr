using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NetLpr.Core.Datas;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Messaging;

namespace NetLpr.Core.Services.Sources
{
    public class RtspSourceContext : IDisposable
    {
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly ConcurrentDictionary<string, DoubleBufferedStreamQueue> _buffers
            = new ConcurrentDictionary<string, DoubleBufferedStreamQueue>();

        private readonly ConcurrentDictionary<string, Action<IDecodedFrame>> _eventHandlers
            = new ConcurrentDictionary<string, Action<IDecodedFrame>>();

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _streamCts
            = new ConcurrentDictionary<string, CancellationTokenSource>();
        public RtspSourceContext(ILogger logger)
        {
            _logger = logger;
            
        }

        public void Register(string id, Action<IDecodedFrame> handler)
        {
            _eventHandlers[id] = handler;
            var buffer = new DoubleBufferedStreamQueue();
            _buffers[id] = buffer;

            var streamCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
            _streamCts[id] = streamCts;

            Task.Factory.StartNew(
                () => buffer.ProcessAsync(frame =>
                {
                    if (_eventHandlers.TryGetValue(id, out var h) && h != null)
                    {
                        try { h.Invoke(frame); }
                        catch (Exception ex) { _logger.LogError(ex, $"Handler Error [{id}]"); }
                    }
                }, streamCts.Token),
                streamCts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        public void Unregister(string id)
        {
            _eventHandlers.TryRemove(id, out _);
            _buffers.TryRemove(id, out _);

            if (_streamCts.TryRemove(id, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        public void Enqueue(string id, IDecodedFrame data)
        {
            if (data == null) return;

            if (_buffers.TryGetValue(id, out var buffer))
            {
                buffer.Enqueue(data);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();

            foreach (var streamCts in _streamCts.Values)
            {
                streamCts.Cancel();
                streamCts.Dispose();
            }

            _buffers.Clear();
            _eventHandlers.Clear();
            _streamCts.Clear();
            _cts.Dispose();
        }


        private class DoubleBufferedStreamQueue
        {
            private readonly Channel<IDecodedFrame> _channel;

            public DoubleBufferedStreamQueue()
            {

                _channel = Channel.CreateUnbounded<IDecodedFrame>(new UnboundedChannelOptions
                {
                    SingleWriter = false,
                    SingleReader = false,
                    AllowSynchronousContinuations = false
                });
            }

            public void Enqueue(IDecodedFrame frame)
            {
                _channel.Writer.TryWrite(frame);
            }

            public async Task ProcessAsync(Action<IDecodedFrame> processAction, CancellationToken ct)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var frame in _channel.Reader.ReadAllAsync(ct))
                        {
                            if (frame == null) continue;

                            try
                            {
                                processAction(frame);
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }, ct);
            }
        }
    }
}