using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Systems.Condition
{
    /// <summary>
    /// Drives PlayerConditionState from the world clock (BL-P1 S7). ShortTick (every game minute)
    /// handles regen/drift that needs per-minute granularity: stamina, wet gain/dry, body
    /// temperature. LongTick (every 10 minutes) handles slower accrual: hunger/thirst/fatigue,
    /// the exposure status chain, and starvation/sickness health decay. Also reacts to
    /// TravelCompleted for the flat fatigue cost per trip — the only other Condition input this
    /// sprint besides the tick loop and item UseEffects (handled in UseItemCommand).
    /// </summary>
    public sealed class ConditionSystem
    {
        private readonly GameContext _ctx;

        public ConditionSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeShort(OnShortTick);
            ctx.Clock.SubscribeLong(OnLongTick);
            ctx.Events.Subscribe<TravelCompleted>(OnTravelCompleted);
        }

        private bool IsRaining() =>
            _ctx.Definitions.TryGetDisasterPhase(_ctx.World.CurrentDisasterPhase, out var phase) && phase.RainIntensity > 0;

        private bool IsAtShelter() =>
            _ctx.Definitions.TryGetLocation(_ctx.World.Player.CurrentLocationId, out var loc) && loc.IsShelter;

        private void OnShortTick(long minute)
        {
            PlayerConditionState c = _ctx.World.Player.Condition;
            ConditionBalance cfg = _ctx.Definitions.Balance.Condition;
            bool atShelter = IsAtShelter();
            bool raining = IsRaining();

            float wet = ConditionOps.GetStatusSeverity(c, ConditionOps.StatusWet);
            if (atShelter) wet -= cfg.WetDryPerMinuteAtShelter;
            else if (raining) wet += cfg.WetGainPerMinuteInRain;
            ConditionOps.SetStatusSeverity(c, ConditionOps.StatusWet, ConditionOps.Clamp(wet), minute);

            if (raining && wet > cfg.WetThresholdForTempDrift)
                c.BodyTemperatureC -= cfg.BodyTempDriftDownPerMinute;
            else if (atShelter)
                c.BodyTemperatureC += cfg.BodyTempRegenAtShelterPerMinute;

            UpdateColdStatus(c, cfg, minute);

            bool halvedRegen = ConditionOps.GetStatusSeverity(c, ConditionOps.StatusBlackWaterExposure) > 0f
                || ConditionOps.GetStatusSeverity(c, ConditionOps.StatusSick) > 0f;
            float regen = cfg.StaminaRegenPerMinute * (halvedRegen ? cfg.StaminaRegenHalvedMultiplier : 1f);
            ConditionOps.ApplyStamina(c, regen);

            _ctx.Events.Publish(new ConditionChanged(_ctx.World.Player.ActorId));
        }

        private static void UpdateColdStatus(PlayerConditionState c, ConditionBalance cfg, long minute)
        {
            bool isCold = ConditionOps.GetStatusSeverity(c, ConditionOps.StatusCold) > 0f;
            if (!isCold && c.BodyTemperatureC < cfg.ColdBodyTempThreshold)
                ConditionOps.SetStatusSeverity(c, ConditionOps.StatusCold, 100f, minute);
            else if (isCold && c.BodyTemperatureC > cfg.ColdClearBodyTempThreshold)
                c.StatusEffects.Remove(ConditionOps.StatusCold);
        }

        private void OnLongTick(long minute)
        {
            PlayerConditionState c = _ctx.World.Player.Condition;
            ConditionBalance cfg = _ctx.Definitions.Balance.Condition;
            const float longTickHours = 10f / 60f;

            ConditionOps.ApplyThirst(c, cfg.ThirstPerHour * longTickHours);
            ConditionOps.ApplyHunger(c, cfg.HungerPerHour * longTickHours);
            ConditionOps.ApplyFatigue(c, cfg.FatiguePerLongTick);

            ConditionOps.ApplyExposureStatusChain(c, "black_water", minute, cfg);

            if (c.Hunger >= 100f || c.Thirst >= 100f)
                ConditionOps.ApplyHealth(c, -cfg.StarvationHealthDecayPerLongTick, cfg.StarvationHealthFloor);

            if (ConditionOps.GetStatusSeverity(c, ConditionOps.StatusSick) > 0f)
                ConditionOps.ApplyHealth(c, -cfg.SickHealthDecayPerLongTick);

            ConditionOps.RecomputeIncapacitation(c, cfg);

            _ctx.Events.Publish(new ConditionChanged(_ctx.World.Player.ActorId));
        }

        private void OnTravelCompleted(TravelCompleted evt) =>
            ConditionOps.ApplyFatigue(_ctx.World.Player.Condition, _ctx.Definitions.Balance.Condition.FatiguePerTravel);
    }
}
