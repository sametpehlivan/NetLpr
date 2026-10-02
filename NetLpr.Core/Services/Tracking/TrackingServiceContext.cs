using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Messaging;
using NetLpr.Core.Values.Request;

namespace NetLpr.Core.Services.Tracking
{
    public class TrackingServiceContext : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, Action<TrackedInfo>> _eventHandlers = new();

        private readonly Channel<(string ServiceId, TrackedInfo Data)> _channel;
        private readonly CancellationTokenSource _cts = new();
        private IMessageBus _messageBus;

        public TrackingServiceContext(ILogger logger, IMessageBus messageBus)
        {
            _messageBus = messageBus;
            _logger = logger;

            _channel = Channel.CreateUnbounded<(string ServiceId, TrackedInfo Data)>(new UnboundedChannelOptions
            {
                SingleWriter = false, 
                SingleReader = true,  
                AllowSynchronousContinuations = false
            });

            Task.Run(ProcessQueueAsync, _cts.Token);
        }

        public void Register(string serviceId, Action<TrackedInfo> handler)
        {
            _eventHandlers[serviceId] = handler;
        }

        public void Unregister(string serviceId)
        {
            _eventHandlers.TryRemove(serviceId, out _);
        }

        public void Enqueue(string serviceId, TrackedInfo data)
        {
            if (data != null && _eventHandlers.ContainsKey(serviceId))
            {
              
                _channel.Writer.TryWrite((serviceId, data));
            }
        }

        private async Task ProcessQueueAsync()
        {
            try
            {
                await foreach (var item in _channel.Reader.ReadAllAsync(_cts.Token))
                {
                    if (_eventHandlers.TryGetValue(item.ServiceId, out var handler))
                    {
                        try
                        {
                            handler.Invoke(item.Data);
                            _messageBus.Publish(new TrackedInfoCompletedRequest() { TrackedInfo = item.Data });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Tracking Service Handler Error [{item.ServiceId}]");
                        }
                        finally
                        {
                            item.Data.Dispose();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} - ProcessQueue Error");
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _channel.Writer.TryComplete();
            _eventHandlers.Clear();
            _cts.Dispose();
        }
    }
}