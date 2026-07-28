using System.IO;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Shelter;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class ShelterEventSystemTests
    {
        DefinitionRegistry definitions;
        WorldState world;
        EventBus events;
        RngStream rng;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));
            world = new WorldState { MasterSeed = 1UL };
            events = new EventBus();
            rng = new RngStream(1UL);
        }

        void ApplyAt(DisasterPhase phase) =>
            ShelterEventSystem.ApplyLongTick(world, definitions, rng, events, phase);

        [Test]
        public void DrainBackflow_TriggersAtRouteClosure_WithGuaranteedChance()
        {
            definitions.Balance.Shelter.DrainBackflowTriggerChancePercent = 100f;
            ShelterEventTriggered? received = null;
            events.Subscribe<ShelterEventTriggered>(e => received = e);

            ApplyAt(DisasterPhase.RouteClosure);

            Assert.IsTrue(world.Shelter.DrainBackflowActive);
            Assert.AreEqual("drain_backflow", received?.EventId);
        }

        [Test]
        public void DrainBackflow_NeverTriggers_BeforeRouteClosure()
        {
            definitions.Balance.Shelter.DrainBackflowTriggerChancePercent = 100f;

            ApplyAt(DisasterPhase.BlackRain);

            Assert.IsFalse(world.Shelter.DrainBackflowActive);
        }

        [Test]
        public void StorageFloodRisk_ActivatesFlag_WhenCriticalAndStorageNonEmpty()
        {
            world.Shelter.WaterIntrusion = definitions.Balance.Shelter.CriticalThreshold;
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            InventoryOps.AddItem(storage, definitions, "item_battery", 1);
            definitions.Balance.Shelter.StorageFloodLossChancePercent = 0f; // chỉ test cờ, không test mất đồ ở đây

            ApplyAt(DisasterPhase.Dry);

            Assert.IsTrue(world.Shelter.StorageFloodRiskActive);
        }

        [Test]
        public void StorageFloodRisk_ProtectedByElevatedStorage_NeverActivates()
        {
            world.Shelter.WaterIntrusion = definitions.Balance.Shelter.CriticalThreshold;
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            InventoryOps.AddItem(storage, definitions, "item_battery", 1);
            world.Shelter.PlacedModules["slot_upper_living_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.ElevatedStorage };

            ApplyAt(DisasterPhase.Dry);

            Assert.IsFalse(world.Shelter.StorageFloodRiskActive);
        }

        [Test]
        public void StorageFloodRisk_GuaranteedChance_RemovesOneStack()
        {
            world.Shelter.WaterIntrusion = definitions.Balance.Shelter.CriticalThreshold;
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            InventoryOps.AddItem(storage, definitions, "item_battery", 1);
            definitions.Balance.Shelter.StorageFloodLossChancePercent = 100f;

            ApplyAt(DisasterPhase.Dry);

            Assert.AreEqual(0, storage.Count);
        }

        [Test]
        public void StorageFloodRisk_ClearsFlag_WhenBelowCritical()
        {
            world.Shelter.StorageFloodRiskActive = true;
            world.Shelter.WaterIntrusion = 0f;

            ApplyAt(DisasterPhase.Dry);

            Assert.IsFalse(world.Shelter.StorageFloodRiskActive);
        }

        [Test]
        public void PumpJam_TriggersOnlyWhenPoweredAndNotAlreadyJammed()
        {
            definitions.Balance.Shelter.PumpJamChancePercent = 100f;
            var pump = new BuiltModuleState { ModuleId = ShelterModuleIds.Pump, Powered = true };
            world.Shelter.PlacedModules["slot_utility_area_1"] = pump;

            ApplyAt(DisasterPhase.Dry);

            Assert.IsTrue(pump.IsJammed);
        }

        [Test]
        public void PumpJam_NotPowered_NeverJams()
        {
            definitions.Balance.Shelter.PumpJamChancePercent = 100f;
            var pump = new BuiltModuleState { ModuleId = ShelterModuleIds.Pump, Powered = false };
            world.Shelter.PlacedModules["slot_utility_area_1"] = pump;

            ApplyAt(DisasterPhase.Dry);

            Assert.IsFalse(pump.IsJammed);
        }
    }
}
