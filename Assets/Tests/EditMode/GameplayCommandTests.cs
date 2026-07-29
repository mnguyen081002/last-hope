using System.IO;
using System.Linq;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Systems.Commands;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class GameplayCommandTests
    {
        const string DrinkShelf1 = "searchpoint_drink_shelf_1";
        const string RouteShelterStore = "route_shelter_store";

        DefinitionRegistry definitions;
        WorldState world;
        GameContext context;
        CommandProcessor processor;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));

            world = new WorldState { MasterSeed = 7UL };
            world.Player.CurrentLocationId = "location_shelter";
            world.Player.Inventory.CapacityKg = definitions.Balance.Inventory.BackpackCapacityKg;
            world.Player.Inventory.CapacityLiters = definitions.Balance.Inventory.BackpackCapacityLiters;

            var events = new EventBus();
            context = new GameContext
            {
                World = world,
                Definitions = definitions,
                Events = events,
                Rng = new RngService(world.MasterSeed, world.RngStreams),
                Ticks = new TickScheduler(world, events),
            };
            processor = new CommandProcessor(context);
        }

        // ---------- OpenSearchPointCommand ----------

        [Test]
        public void OpenSearchPoint_WrongLocation_IsRejected()
        {
            world.Player.CurrentLocationId = "location_shelter"; // drink_shelf_1 ở convenience_store

            var result = processor.Submit(new OpenSearchPointCommand(DrinkShelf1));

            Assert.AreEqual(CommandErrorCode.WrongLocation, result.Error);
        }

        [Test]
        public void OpenSearchPoint_CorrectLocation_RollsLoot()
        {
            world.Player.CurrentLocationId = "location_convenience_store";

            var result = processor.Submit(new OpenSearchPointCommand(DrinkShelf1));

            Assert.IsTrue(result.Success);
            var state = world.Locations["location_convenience_store"].SearchPoints[DrinkShelf1];
            Assert.IsTrue(state.Rolled);
        }

        // ---------- TakeAllFromSearchPointCommand ----------

        [Test]
        public void TakeAllFromSearchPoint_BeforeOpen_IsRejected()
        {
            world.Player.CurrentLocationId = "location_convenience_store";

            var result = processor.Submit(new TakeAllFromSearchPointCommand(DrinkShelf1));

            Assert.AreEqual(CommandErrorCode.NotAllowedNow, result.Error);
        }

        [Test]
        public void TakeAllFromSearchPoint_AfterOpen_MovesItemsToPlayer()
        {
            world.Player.CurrentLocationId = "location_convenience_store";
            processor.Submit(new OpenSearchPointCommand(DrinkShelf1));

            var take = new TakeAllFromSearchPointCommand(DrinkShelf1);
            var result = processor.Submit(take);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(take.TookEverything);
            Assert.AreEqual(3, InventoryOps.CountOf(world.Player.Inventory, "item_water_bottle"));
        }

        // ---------- TransferItemCommand ----------

        [Test]
        public void Transfer_PlayerToShelterStorage_MovesItem()
        {
            InventoryOps.AddItem(world.Player.Inventory, definitions, "item_battery", 2);

            var command = new TransferItemCommand(
                InventoryOwner.Player, InventoryOwner.ShelterStorage("location_shelter"),
                "item_battery", 2);
            var result = processor.Submit(command);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, InventoryOps.CountOf(world.Player.Inventory, "item_battery"));
            Assert.AreEqual(2, InventoryOps.CountOf(
                world.GetOrCreateLocation("location_shelter").StorageContainer, "item_battery"));
        }

        [Test]
        public void Transfer_ShelterStorageToPlayer_RoundTrips()
        {
            var storage = world.GetOrCreateLocation("location_shelter").StorageContainer;
            InventoryOps.AddItem(storage, definitions, "item_canned_food", 3);

            var result = processor.Submit(new TransferItemCommand(
                InventoryOwner.ShelterStorage("location_shelter"), InventoryOwner.Player,
                "item_canned_food", 3));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, InventoryOps.CountOf(world.Player.Inventory, "item_canned_food"));
        }

        [Test]
        public void Transfer_MoreThanAvailable_IsRejected_DoesNotMutate()
        {
            InventoryOps.AddItem(world.Player.Inventory, definitions, "item_battery", 1);

            var result = processor.Submit(new TransferItemCommand(
                InventoryOwner.Player, InventoryOwner.ShelterStorage("location_shelter"),
                "item_battery", 5));

            Assert.AreEqual(CommandErrorCode.ItemNotFound, result.Error);
            Assert.AreEqual(1, InventoryOps.CountOf(world.Player.Inventory, "item_battery"));
        }

        [Test]
        public void Transfer_PastHardCap_IsRejected()
        {
            world.Player.Inventory.CapacityKg = 1f;
            var storage = world.GetOrCreateLocation("location_shelter").StorageContainer;
            InventoryOps.AddItem(storage, definitions, "item_toolbox", 1); // 8kg > hard cap của 1kg

            var result = processor.Submit(new TransferItemCommand(
                InventoryOwner.ShelterStorage("location_shelter"), InventoryOwner.Player,
                "item_toolbox", 1));

            Assert.AreEqual(CommandErrorCode.NotEnoughCapacity, result.Error);
        }

        [Test]
        public void Transfer_FromUnopenedSearchPoint_IsRejected()
        {
            world.Player.CurrentLocationId = "location_convenience_store";
            // Chưa Open — search point chưa Rolled.

            var result = processor.Submit(new TransferItemCommand(
                InventoryOwner.SearchPoint(DrinkShelf1), InventoryOwner.Player,
                "item_water_bottle", 1));

            Assert.AreEqual(CommandErrorCode.ItemNotFound, result.Error);
        }

        [Test]
        public void Transfer_TwoHandCarryItem_RoutesToCarriedSlot_NotStorage()
        {
            var storage = world.GetOrCreateLocation("location_shelter").StorageContainer;
            InventoryOps.AddItem(storage, definitions, "item_water_container_20l", 1);

            var result = processor.Submit(new TransferItemCommand(
                InventoryOwner.ShelterStorage("location_shelter"), InventoryOwner.Player,
                "item_water_container_20l", 1));

            Assert.IsTrue(result.Success);
            Assert.AreEqual("item_water_container_20l", world.Player.Inventory.CarriedObjectItemId);
        }

        [Test]
        public void Transfer_DropThenPickUp_RoundTrips()
        {
            InventoryOps.AddItem(world.Player.Inventory, definitions, "item_canned_food", 2);

            processor.Submit(new TransferItemCommand(
                InventoryOwner.Player, InventoryOwner.DroppedItems("location_shelter"),
                "item_canned_food", 2));
            Assert.AreEqual(0, InventoryOps.CountOf(world.Player.Inventory, "item_canned_food"));

            var pickUp = processor.Submit(new TransferItemCommand(
                InventoryOwner.DroppedItems("location_shelter"), InventoryOwner.Player,
                "item_canned_food", 2));

            Assert.IsTrue(pickUp.Success);
            Assert.AreEqual(2, InventoryOps.CountOf(world.Player.Inventory, "item_canned_food"));
        }

        // ---------- BeginTravelCommand ----------

        [Test]
        public void BeginTravel_RouteNotConnected_IsRejected()
        {
            world.Player.CurrentLocationId = "location_convenience_store"; // route nối shelter<->store, thử route khác không liên quan

            var result = processor.Submit(new BeginTravelCommand("route_khong_ton_tai"));

            Assert.AreEqual(CommandErrorCode.UnknownDefinition, result.Error);
        }

        [Test]
        public void BeginTravel_Valid_AdvancesTimeAndPublishesLocationChanged()
        {
            LocationChanged? received = null;
            context.Events.Subscribe<LocationChanged>(e => received = e);

            var result = processor.Submit(new BeginTravelCommand(RouteShelterStore));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(25, world.WorldTimeMinutes);
            Assert.IsTrue(received.HasValue);
            Assert.AreEqual("location_shelter", received.Value.FromLocationId);
            Assert.AreEqual("location_convenience_store", received.Value.ToLocationId);
        }

        [Test]
        public void BeginTravel_ImpassableRoute_IsRejected_DoesNotMutate()
        {
            world.GetOrCreateRoute(RouteShelterStore).Flood = LastHope.Core.State.FloodState.Impassable;
            long timeBefore = world.WorldTimeMinutes;

            var result = processor.Submit(new BeginTravelCommand(RouteShelterStore));

            Assert.AreEqual(CommandErrorCode.NotAllowedNow, result.Error);
            Assert.AreEqual(timeBefore, world.WorldTimeMinutes);
            Assert.AreEqual("location_shelter", world.Player.CurrentLocationId);
        }

        [Test]
        public void BeginTravel_DeepFlood_StillPassable_AppliesCrossingCost()
        {
            world.GetOrCreateRoute(RouteShelterStore).Flood = LastHope.Core.State.FloodState.Deep;

            var result = processor.Submit(new BeginTravelCommand(RouteShelterStore));

            Assert.IsTrue(result.Success);
            Assert.Less(world.Player.Stamina, 100f);
            Assert.Greater(world.Player.BlackWaterExposure, 0f);
        }

        // ---------- EquipItemCommand / UnequipItemCommand ----------

        [Test]
        public void EquipItem_Valid_MovesToEquippedSlot()
        {
            InventoryOps.AddItem(world.Player.Inventory, definitions, "item_jacket", 1);

            var result = processor.Submit(new EquipItemCommand("item_jacket"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual("item_jacket", world.Player.Equipped[LastHope.Data.Definitions.EquipSlot.Body]);
        }

        [Test]
        public void EquipItem_NotEquipment_IsRejected()
        {
            InventoryOps.AddItem(world.Player.Inventory, definitions, "item_water_bottle", 1);

            var result = processor.Submit(new EquipItemCommand("item_water_bottle"));

            Assert.AreEqual(CommandErrorCode.InvalidTarget, result.Error);
        }

        [Test]
        public void EquipItem_NotInInventory_IsRejected()
        {
            var result = processor.Submit(new EquipItemCommand("item_jacket"));

            Assert.AreEqual(CommandErrorCode.ItemNotFound, result.Error);
        }

        [Test]
        public void UnequipItem_EmptySlot_IsRejected()
        {
            var result = processor.Submit(new UnequipItemCommand(LastHope.Data.Definitions.EquipSlot.Body));

            Assert.AreEqual(CommandErrorCode.InvalidTarget, result.Error);
        }

        [Test]
        public void UnequipItem_Valid_ReturnsToInventory()
        {
            InventoryOps.AddItem(world.Player.Inventory, definitions, "item_jacket", 1);
            processor.Submit(new EquipItemCommand("item_jacket"));

            var result = processor.Submit(new UnequipItemCommand(LastHope.Data.Definitions.EquipSlot.Body));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, InventoryOps.CountOf(world.Player.Inventory, "item_jacket"));
        }

        // ---------- StartConstructionCommand / CancelConstructionCommand / DismantleModuleCommand ----------

        void GiveShelterMaterials(string moduleId)
        {
            var module = definitions.GetModule(moduleId);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            foreach (var pair in module.Materials)
                InventoryOps.AddItem(storage, definitions, pair.Key, pair.Value);
        }

        const string UtilityZone = "utility_area";
        const float ZoneX = 5f, ZoneY = 5f; // trong bounds utility_area (0,0)-(10,8).

        [Test]
        public void StartConstruction_NotAtShelter_IsRejected()
        {
            world.Player.CurrentLocationId = "location_convenience_store";
            GiveShelterMaterials(ShelterModuleIds.Pump);

            var result = processor.Submit(
                new StartConstructionCommand(UtilityZone, ZoneX, ZoneY, ShelterModuleIds.Pump));

            Assert.AreEqual(CommandErrorCode.WrongLocation, result.Error);
        }

        [Test]
        public void StartConstruction_Valid_DeductsMaterials_CreatesConstruction()
        {
            GiveShelterMaterials(ShelterModuleIds.Pump);

            var result = processor.Submit(
                new StartConstructionCommand(UtilityZone, ZoneX, ZoneY, ShelterModuleIds.Pump));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(UtilityZone, world.Shelter.Construction.ZoneId);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            Assert.AreEqual(0, InventoryOps.CountOf(storage, "item_pump_part"));
        }

        [Test]
        public void CancelConstruction_Valid_ClearsConstruction()
        {
            GiveShelterMaterials(ShelterModuleIds.Pump);
            processor.Submit(new StartConstructionCommand(UtilityZone, ZoneX, ZoneY, ShelterModuleIds.Pump));

            var result = processor.Submit(new CancelConstructionCommand());

            Assert.IsTrue(result.Success);
            Assert.IsNull(world.Shelter.Construction);
        }

        [Test]
        public void DismantleModule_Valid_RemovesModule_AddsPackedItemToStorage()
        {
            world.Shelter.PlacedModules["placed_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Pump, ZoneId = UtilityZone };

            var result = processor.Submit(new DismantleModuleCommand("placed_1"));

            Assert.IsTrue(result.Success);
            Assert.IsFalse(world.Shelter.PlacedModules.ContainsKey("placed_1"));
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            Assert.AreEqual(1, InventoryOps.CountOf(storage, "item_packed_pump"));
        }

        // ---------- RedeployModuleCommand ----------

        [Test]
        public void RedeployModule_NoPackedItem_IsRejected()
        {
            var result = processor.Submit(
                new RedeployModuleCommand(UtilityZone, ZoneX, ZoneY, ShelterModuleIds.Pump));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.ItemNotFound, result.Error);
        }

        [Test]
        public void RedeployModule_Valid_CreatesModuleInstantly_NoConstruction()
        {
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            InventoryOps.AddItem(storage, definitions, "item_packed_pump", 1);

            var result = processor.Submit(
                new RedeployModuleCommand(UtilityZone, ZoneX, ZoneY, ShelterModuleIds.Pump));

            Assert.IsTrue(result.Success);
            Assert.IsNull(world.Shelter.Construction);
            Assert.AreEqual(0, InventoryOps.CountOf(storage, "item_packed_pump"));
            Assert.IsTrue(world.Shelter.PlacedModules.Values.Any(m => m.ModuleId == ShelterModuleIds.Pump));
        }

        // ---------- SetPowerPriorityCommand ----------

        [Test]
        public void SetPowerPriority_Valid_UpdatesPriority()
        {
            world.Shelter.PlacedModules["placed_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Pump, Priority = PowerPriority.Normal };

            var result = processor.Submit(new SetPowerPriorityCommand("placed_1", PowerPriority.Critical));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(PowerPriority.Critical, world.Shelter.PlacedModules["placed_1"].Priority);
        }

        // ---------- ResolveDrainBackflowCommand ----------

        [Test]
        public void ResolveDrainBackflow_NotActive_IsRejected()
        {
            var result = processor.Submit(new ResolveDrainBackflowCommand());

            Assert.AreEqual(CommandErrorCode.NotAllowedNow, result.Error);
        }

        [Test]
        public void ResolveDrainBackflow_Active_ResolvesAndAdvancesTime()
        {
            world.Shelter.DrainBackflowActive = true;

            var result = processor.Submit(new ResolveDrainBackflowCommand());

            Assert.IsTrue(result.Success);
            Assert.IsFalse(world.Shelter.DrainBackflowActive);
            Assert.AreEqual((long)definitions.Balance.Shelter.DrainBackflowResolveMinutes, world.WorldTimeMinutes);
        }

        // ---------- RepairPumpJamCommand ----------

        [Test]
        public void RepairPumpJam_NotJammed_IsRejected()
        {
            world.Shelter.PlacedModules["slot_utility_area_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Pump, IsJammed = false };

            var result = processor.Submit(new RepairPumpJamCommand());

            Assert.AreEqual(CommandErrorCode.NotAllowedNow, result.Error);
        }

        [Test]
        public void RepairPumpJam_Jammed_RepairsAndAdvancesTime()
        {
            world.Shelter.PlacedModules["slot_utility_area_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Pump, IsJammed = true };

            var result = processor.Submit(new RepairPumpJamCommand());

            Assert.IsTrue(result.Success);
            Assert.IsFalse(world.Shelter.PlacedModules["slot_utility_area_1"].IsJammed);
            Assert.AreEqual((long)definitions.Balance.Shelter.PumpJamResolveMinutes, world.WorldTimeMinutes);
        }

        // ---------- SleepCommand ----------

        [Test]
        public void Sleep_NotAtShelter_IsRejected()
        {
            world.Player.CurrentLocationId = "location_convenience_store";

            var result = processor.Submit(new SleepCommand(6f));

            Assert.AreEqual(CommandErrorCode.WrongLocation, result.Error);
        }

        [Test]
        public void Sleep_OutOfRange_IsRejected()
        {
            var result = processor.Submit(new SleepCommand(999f));

            Assert.AreEqual(CommandErrorCode.InvalidTarget, result.Error);
        }

        [Test]
        public void Sleep_Valid_AdvancesTime_RecoversFatigue()
        {
            world.Player.Fatigue = 80f;

            var result = processor.Submit(new SleepCommand(6f));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(6 * 60, world.WorldTimeMinutes);
            Assert.Less(world.Player.Fatigue, 80f);
        }
    }
}
