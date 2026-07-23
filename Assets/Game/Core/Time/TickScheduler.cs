using System;
using System.Collections.Generic;
using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Core.Time
{
    /// <summary>
    /// Owns the minute loop: the ONLY place WorldState.WorldTimeMinutes advances
    /// (technical-specification.md mục 9/§8). Short Tick = every game minute, Long Tick = every
    /// 10th game minute (m % 10 == 0) — both derived from the same counter, so they can never
    /// double-fire or drift apart.
    /// </summary>
    public sealed class TickScheduler
    {
        private readonly WorldState _world;
        private readonly EventBus _bus;
        private readonly List<Action<long>> _shortSubs = new List<Action<long>>();
        private readonly List<Action<long>> _longSubs = new List<Action<long>>();
        private readonly SortedDictionary<long, List<Action<long>>> _thresholds = new SortedDictionary<long, List<Action<long>>>();

        public TickScheduler(WorldState world, EventBus bus)
        {
            _world = world;
            _bus = bus;
        }

        public void SubscribeShort(Action<long> callback) => _shortSubs.Add(callback);
        public void SubscribeLong(Action<long> callback) => _longSubs.Add(callback);

        /// <summary>Fires once, the first time WorldTimeMinutes reaches or passes worldMinute.</summary>
        public void RegisterThreshold(long worldMinute, Action<long> onCrossed)
        {
            if (!_thresholds.TryGetValue(worldMinute, out var list))
            {
                list = new List<Action<long>>();
                _thresholds[worldMinute] = list;
            }
            list.Add(onCrossed);
        }

        /// <summary>Drains up to maxMinutes whole minutes from the clock's bank. Remainder stays banked.</summary>
        public int Advance(SimulationClock clock, int maxMinutes)
        {
            int consumed = 0;
            while (consumed < maxMinutes && clock.TryConsumeMinute())
            {
                AdvanceOneMinute();
                consumed++;
            }
            return consumed;
        }

        /// <summary>Sleep/Travel fast-forward: iterates minute-by-minute, uncapped, clock-independent.</summary>
        public void FastForward(int minutes)
        {
            for (int i = 0; i < minutes; i++) AdvanceOneMinute();
        }

        private void AdvanceOneMinute()
        {
            _world.WorldTimeMinutes++;
            long m = _world.WorldTimeMinutes;

            foreach (var cb in _shortSubs) cb(m);
            if (m % 10 == 0) foreach (var cb in _longSubs) cb(m);

            FireCrossedThresholds(m);

            _bus.Publish(new WorldTimeChanged(m, GameTimeUtil.DayIndex(m), GameTimeUtil.TimeOfDayMinutes(m)));
        }

        private void FireCrossedThresholds(long m)
        {
            if (_thresholds.Count == 0) return;

            var toFire = new List<long>();
            foreach (var kvp in _thresholds)
            {
                if (kvp.Key > m) break; // SortedDictionary enumerates ascending by key
                toFire.Add(kvp.Key);
            }

            foreach (long key in toFire)
            {
                foreach (var cb in _thresholds[key]) cb(key);
                _thresholds.Remove(key);
            }
        }
    }
}
