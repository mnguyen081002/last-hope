using System;
using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Core.Time
{
    /// <summary>
    /// Bơm thời gian vào world. <see cref="AdvanceOneMinute"/> là **nơi duy nhất**
    /// <c>WorldState.WorldTimeMinutes</c> được tăng — mọi đường khác (chơi thường, ngủ,
    /// travel) đều phải đi qua đây để không hệ thống nào bị bỏ tick.
    /// </summary>
    public class TickScheduler
    {
        /// <summary>Số phút giữa hai long tick (hệ thống chậm: nước, điện, NPC).</summary>
        public const int LongTickIntervalMinutes = 10;

        /// <summary>Trần catch-up mỗi lần Advance, tránh spiral of death sau khi treo máy.</summary>
        public const int DefaultMaxCatchUpMinutes = 60;

        readonly WorldState world;
        readonly EventBus events;

        /// <summary>Chạy mỗi phút game.</summary>
        public event Action<long> ShortTick;

        /// <summary>Chạy mỗi <see cref="LongTickIntervalMinutes"/> phút game.</summary>
        public event Action<long> LongTick;

        public TickScheduler(WorldState world, EventBus events)
        {
            this.world = world;
            this.events = events;
        }

        /// <summary>
        /// Tiêu thụ số phút clock vừa bank. Trả về số phút thực sự chạy — phần vượt trần
        /// catch-up bị bỏ (thời gian thực đã trôi nhưng không mô phỏng dồn).
        /// </summary>
        public int Advance(int minutes, int maxCatchUpMinutes = DefaultMaxCatchUpMinutes)
        {
            if (minutes <= 0) return 0;

            int toRun = Math.Min(minutes, maxCatchUpMinutes);
            for (int i = 0; i < toRun; i++) AdvanceOneMinute();
            return toRun;
        }

        /// <summary>
        /// Nhảy thời gian có kiểm soát (ngủ, travel). Vẫn chạy từng phút một để không hệ
        /// thống nào bị bỏ qua — không được cộng thẳng vào WorldTimeMinutes.
        /// </summary>
        public void FastForward(int minutes)
        {
            for (int i = 0; i < minutes; i++) AdvanceOneMinute();
        }

        void AdvanceOneMinute()
        {
            world.WorldTimeMinutes++;
            long now = world.WorldTimeMinutes;

            ShortTick?.Invoke(now);

            bool isLongTick = now % LongTickIntervalMinutes == 0;
            if (isLongTick) LongTick?.Invoke(now);

            events?.Publish(new WorldTimeChanged(now, isLongTick));
        }
    }
}
