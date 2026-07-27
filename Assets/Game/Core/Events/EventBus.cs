using System;
using System.Collections.Generic;

namespace LastHope.Core.Events
{
    /// <summary>Cho phép EventBus dọn kênh mà không cần biết tham số kiểu.</summary>
    public interface IEventChannel
    {
        void Clear();
    }

    /// <summary>
    /// Kênh cho một loại event. Handler lưu trong mảng copy-on-write: publish chỉ duyệt
    /// mảng, nên handler tự gỡ mình lúc đang publish cũng không làm hỏng vòng lặp.
    /// </summary>
    public class EventChannel<T> : IEventChannel where T : struct
    {
        Action<T>[] handlers = Array.Empty<Action<T>>();

        public int HandlerCount => handlers.Length;

        public void Subscribe(Action<T> handler)
        {
            if (handler == null) return;

            var updated = new Action<T>[handlers.Length + 1];
            Array.Copy(handlers, updated, handlers.Length);
            updated[handlers.Length] = handler;
            handlers = updated;
        }

        public void Unsubscribe(Action<T> handler)
        {
            if (handler == null) return;

            int index = Array.IndexOf(handlers, handler);
            if (index < 0) return;

            var updated = new Action<T>[handlers.Length - 1];
            Array.Copy(handlers, 0, updated, 0, index);
            Array.Copy(handlers, index + 1, updated, index, handlers.Length - index - 1);
            handlers = updated;
        }

        public void Publish(T payload)
        {
            var snapshot = handlers;
            for (int i = 0; i < snapshot.Length; i++) snapshot[i](payload);
        }

        public void Clear() => handlers = Array.Empty<Action<T>>();
    }

    /// <summary>Điểm phát/nhận tín hiệu giữa các hệ thống, tránh gọi thẳng lẫn nhau.</summary>
    public class EventBus
    {
        readonly Dictionary<Type, IEventChannel> channels = new();

        public EventChannel<T> Channel<T>() where T : struct
        {
            if (channels.TryGetValue(typeof(T), out var existing))
            {
                return (EventChannel<T>)existing;
            }

            var channel = new EventChannel<T>();
            channels[typeof(T)] = channel;
            return channel;
        }

        public void Subscribe<T>(Action<T> handler) where T : struct =>
            Channel<T>().Subscribe(handler);

        public void Unsubscribe<T>(Action<T> handler) where T : struct =>
            Channel<T>().Unsubscribe(handler);

        public void Publish<T>(T payload) where T : struct =>
            Channel<T>().Publish(payload);

        public void ClearAll()
        {
            foreach (var channel in channels.Values) channel.Clear();
        }
    }
}
