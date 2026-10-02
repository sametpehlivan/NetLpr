using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Services.Messaging
{
    public interface IMessageBus : IAsyncDisposable
    {
        void Subscribe<T>(string subscriptionId, Action<T> onMessage);
        void Subscribe<T>(string subscriptionId, Func<T, Task> onMessageAsync);
        void Unsubscribe<T>(string subscriptionId);
        void Publish<T>(T message) where T : class;
    }
}
