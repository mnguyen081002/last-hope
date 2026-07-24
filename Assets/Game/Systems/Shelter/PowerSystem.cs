using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;

namespace LastHope.Systems.Shelter
{
    /// <summary>
    /// Resolves power allocation every long-tick (S12): City Grid (flag "grid_down" — nothing sets
    /// it until S17's Grid Failure event, so the grid is always available for now) then Battery,
    /// highest priority first. Flips ModuleState.Active for power-consuming modules only — modules
    /// with PowerDemand 0 (Barrier, Elevated Storage) are never touched here.
    /// </summary>
    public sealed class PowerSystem
    {
        private readonly GameContext _ctx;

        public PowerSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeLong(OnLongTick);
        }

        private void OnLongTick(long minute)
        {
            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)) return;

            var demands = new List<PowerDemandEntry>();
            foreach (var module in shelter.Modules.Values)
            {
                if (!_ctx.Definitions.TryGetModule(module.ModuleId, out var def) || def.PowerDemand <= 0f) continue;
                var priority = shelter.Power.Priorities.TryGetValue(module.InstanceId, out var p) ? p : Core.State.PowerPriority.Normal;
                demands.Add(new PowerDemandEntry(module.InstanceId, def.PowerDemand, priority));
            }

            bool gridAvailable = !(_ctx.World.PersistentFlags.TryGetValue("grid_down", out var down) && down);
            var result = PowerRules.Allocate(gridAvailable, shelter.Power.BatteryCharge, demands, _ctx.Definitions.Balance.Power);

            bool changed = false;
            foreach (var kvp in result.Powered)
            {
                var module = shelter.Modules[kvp.Key];
                if (module.Active != kvp.Value) changed = true;
                module.Active = kvp.Value;
            }
            shelter.Power.BatteryCharge = result.NewBatteryCharge;

            if (changed) _ctx.Events.Publish(new PowerStateChanged(shelterId));
        }
    }
}
