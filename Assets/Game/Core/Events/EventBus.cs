using System;
using System.Collections.Generic;
using System.Linq;

namespace LastHope.Core.Events
{
    /// <summary>
    /// Typed pub/sub for struct events, zero boxing. Copy-on-write handler arrays: Subscribe/
    /// Unsubscribe rebuild the array immediately (rare, e.g. at scene load); Publish iterates a
    /// snapshot reference so handlers may safely unsubscribe themselves mid-publish.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

        public void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent => Channel<T>().Subscribe(handler);
        public void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent => Channel<T>().Unsubscribe(handler);
        public void Publish<T>(in T evt) where T : struct, IGameEvent => Channel<T>().Publish(evt);

        private EventChannel<T> Channel<T>() where T : struct, IGameEvent
        {
            var type = typeof(T);
            if (!_channels.TryGetValue(type, out var channel))
            {
                channel = new EventChannel<T>();
                _channels[type] = channel;
            }
            return (EventChannel<T>)channel;
        }

        private sealed class EventChannel<T> where T : struct, IGameEvent
        {
            private Action<T>[] _handlers = Array.Empty<Action<T>>();

            public void Subscribe(Action<T> handler) => _handlers = _handlers.Append(handler).ToArray();
            public void Unsubscribe(Action<T> handler) => _handlers = _handlers.Where(h => h != handler).ToArray();

            public void Publish(in T evt)
            {
                var snapshot = _handlers;
                for (int i = 0; i < snapshot.Length; i++) snapshot[i](evt);
            }
        }
    }
}
