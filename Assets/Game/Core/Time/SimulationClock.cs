namespace LastHope.Core.Time
{
    /// <summary>
    /// Frame-rate-independent real-to-game time accumulation (technical-specification.md mục 9/§7:
    /// 1 real second = 5 game seconds). Banks whole game-seconds; TickScheduler drains whole minutes.
    /// The sub-minute remainder is transient and intentionally not part of saved state.
    /// </summary>
    public sealed class SimulationClock
    {
        public const double GameSecondsPerRealSecond = 5.0;
        private const decimal GameSecondsPerMinute = 60m;

        // decimal, not double: summing thousands of small per-frame deltas in binary floating
        // point accumulates rounding error (observed: 24h simulation landing 1 minute short).
        // decimal's much higher precision keeps that error far below a minute boundary.
        private decimal _bankedGameSeconds;

        public double PendingGameSeconds => (double)_bankedGameSeconds;

        public void AccumulateRealSeconds(double realDeltaSeconds)
        {
            if (realDeltaSeconds <= 0) return;
            _bankedGameSeconds += (decimal)realDeltaSeconds * (decimal)GameSecondsPerRealSecond;
        }

        /// <summary>Consumes one whole game-minute from the bank if available.</summary>
        public bool TryConsumeMinute()
        {
            if (_bankedGameSeconds < GameSecondsPerMinute) return false;
            _bankedGameSeconds -= GameSecondsPerMinute;
            return true;
        }
    }
}
