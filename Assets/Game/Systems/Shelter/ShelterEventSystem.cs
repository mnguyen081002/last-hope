using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Shelter
{
    /// <summary>
    /// Ba Shelter Event của P3 (BL-P3-14/15/16). Chance tự đề xuất 2026-07-28, chưa qua
    /// playtest — xem docs/plans/2026-07-28-p3-shelter-loop.md.
    /// </summary>
    public static class ShelterEventSystem
    {
        public static void ApplyLongTick(
            WorldState world, DefinitionRegistry definitions, RngStream rng, EventBus events, DisasterPhase phase)
        {
            var shelter = world.Shelter;
            var balance = definitions.Balance.Shelter;

            ApplyDrainBackflow(shelter, balance, rng, events, phase);
            ApplyStorageFloodRisk(world, shelter, balance, rng, events);
            ApplyPumpJam(shelter, balance, rng, events);
        }

        static void ApplyDrainBackflow(
            ShelterState shelter, ShelterBalance balance, RngStream rng, EventBus events, DisasterPhase phase)
        {
            if (shelter.DrainBackflowActive) return;
            if (phase != DisasterPhase.RouteClosure) return; // giai đoạn cuối, tương ứng Peak/Escalation.
            if (!rng.NextChance(balance.DrainBackflowTriggerChancePercent)) return;

            shelter.DrainBackflowActive = true;
            events?.Publish(new ShelterEventTriggered("drain_backflow"));
        }

        static void ApplyStorageFloodRisk(
            WorldState world, ShelterState shelter, ShelterBalance balance, RngStream rng, EventBus events)
        {
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            bool protectedByElevatedStorage =
                ShelterWaterSystem.FindModule(shelter, ShelterModuleIds.ElevatedStorage) != null;

            bool atRisk = shelter.WaterIntrusion >= balance.CriticalThreshold
                && storage.Count > 0 && !protectedByElevatedStorage;

            if (!atRisk)
            {
                shelter.StorageFloodRiskActive = false;
                return;
            }

            if (!shelter.StorageFloodRiskActive)
            {
                shelter.StorageFloodRiskActive = true;
                events?.Publish(new ShelterEventTriggered("storage_flood_risk"));
            }

            if (rng.NextChance(balance.StorageFloodLossChancePercent))
            {
                storage.RemoveAt(rng.NextInt(0, storage.Count));
            }
        }

        static void ApplyPumpJam(ShelterState shelter, ShelterBalance balance, RngStream rng, EventBus events)
        {
            var pump = ShelterWaterSystem.FindModule(shelter, ShelterModuleIds.Pump);
            if (pump == null || !pump.Powered || pump.IsJammed) return;
            if (!rng.NextChance(balance.PumpJamChancePercent)) return;

            pump.IsJammed = true;
            events?.Publish(new ShelterEventTriggered("pump_jam"));
        }
    }
}
