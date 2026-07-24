using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Data;

namespace LastHope.Systems.Shelter
{
    /// <summary>
    /// Passive Untreated Water intake through the Water Intake Point while it's raining
    /// (main-shelter-design.md §11, S12) — the active Purify batch is player-triggered
    /// (StartPurifyBatchCommand), not automatic, so this system only ever grows WaterStocks.Untreated.
    /// </summary>
    public sealed class WaterSystem
    {
        private readonly GameContext _ctx;

        public WaterSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeLong(OnLongTick);
        }

        private void OnLongTick(long minute)
        {
            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)) return;

            bool raining = _ctx.Definitions.TryGetDisasterPhase(_ctx.World.CurrentDisasterPhase, out var phase) && phase.RainIntensity > 0;
            if (!raining) return;

            WaterBalance cfg = _ctx.Definitions.Balance.Water;
            shelter.WaterStocks.Untreated += cfg.IntakeUntreatedPerHour * (10f / 60f);
            _ctx.Events.Publish(new WaterStocksChanged(shelterId));
        }
    }
}
