using System.Collections.Concurrent;
using System.Threading.Channels;
using NetLpr.Core.Log;

namespace NetLpr.Core.Services.Messaging
{
    public class InMemoryMessageBus : IMessageBus
    {
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, Task>>> _subscriptions = new();
        private readonly Channel<object> _messageQueue;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _processingTask;

        public InMemoryMessageBus(ILogger logger)
        {
            _logger = logger;

            _messageQueue = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = true
            });

            _processingTask = Task.Run(ProcessMessagesAsync);
        }

        public void Subscribe<T>(string subscriptionId, Action<T> onMessage)
        {
            Subscribe<T>(subscriptionId, message =>
            {
                onMessage(message);
                return Task.CompletedTask;
            });
        }

        public void Subscribe<T>(string subscriptionId, Func<T, Task> onMessageAsync)
        {
            var messageType = typeof(T);

            var typeSubscribers = _subscriptions.GetOrAdd(messageType, _ => new ConcurrentDictionary<string, Func<object, Task>>());

            Func<object, Task> wrappedHandler = obj => onMessageAsync((T)obj);

            if (!typeSubscribers.TryAdd(subscriptionId, wrappedHandler))
            {
                _logger?.LogWarning($"Subscriber with ID '{subscriptionId}' is already registered for type [{messageType.Name}].");
            }
        }

        public void Unsubscribe<T>(string subscriptionId)
        {
            if (_subscriptions.TryGetValue(typeof(T), out var typeSubscribers))
            {
                typeSubscribers.TryRemove(subscriptionId, out _);
            }
        }




        public void Publish<T>(T message) where T : class
        {
            if (message == null) return;

            _messageQueue.Writer.TryWrite(message);
        }


        private async Task ProcessMessagesAsync()
        {
            try
            {
                while (await _messageQueue.Reader.WaitToReadAsync(_cts.Token))
                {
                    while (_messageQueue.Reader.TryRead(out var message))
                    {
                        var messageType = message.GetType();

                        if (_subscriptions.TryGetValue(messageType, out var typeSubscribers) && !typeSubscribers.IsEmpty)
                        {
                            foreach (var (subId, handler) in typeSubscribers)
                            {
                                try
                                {
                                    await handler(message);
                                }
                                catch (Exception ex)
                                {
                                    _logger?.LogError($"Error handling message for type [{messageType.Name}] on subscriber '{subId}': {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Critical error in MessageBus processing loop: {ex.Message}");
            }
        }


        public async ValueTask DisposeAsync()
        {
            _messageQueue.Writer.TryComplete();
            _cts.Cancel();

            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
            }

            _subscriptions.Clear();
            _cts.Dispose();
        }
    }
}