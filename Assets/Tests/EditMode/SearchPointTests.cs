using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.Save;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class SearchPointTests
    {
        private static GameContext BuildContext()
        {
            var world = new WorldState { RandomSeed = 12345 };
            world.Player.CurrentLocationId = "location_store";
            var bus = new EventBus();

            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_water"] = new ItemDefinition { Id = "item_water", BaseWeightKg = 0.8f, BaseVolumeLiters = 1f, MaxStackSize = 4 },
            };
            var locations = new Dictionary<string, LocationDefinition>
            {
                ["location_store"] = new LocationDefinition { Id = "location_store" },
            };
            var searchPoints = new Dictionary<string, SearchPointDefinition>
            {
                ["sp_shelf"] = new SearchPointDefinition
                {
                    Id = "sp_shelf",
                    LocationId = "location_store",
                    OpenTimeMinutes = 0,
                    LootTable = new List<LootEntry>
                    {
                        new LootEntry { ItemId = "item_water", Weight = 1, MinQuantity = 3, MaxQuantity = 3 },
                    },
                },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), items, locations, new Dictionary<string, RouteDefinition>(), searchPoints);

            var rng = new RngService(world);
            var scheduler = new TickScheduler(world, bus);
            return new GameContext(world, registry, bus, rng, scheduler);
        }

        [Test]
        public void Open_AtWrongLocation_FailsNotAtLocation()
        {
            var ctx = BuildContext();
            ctx.World.Player.CurrentLocationId = "somewhere_else";
            var processor = new CommandProcessor(ctx);

            var result = processor.Submit(new OpenSearchPointCommand("player", "sp_shelf"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NotAtLocation, result.Code);
        }

        [Test]
        public void Open_RollsOnce_SecondOpenDoesNotReRoll()
        {
            var ctx = BuildContext();
            var processor = new CommandProcessor(ctx);

            processor.Submit(new OpenSearchPointCommand("player", "sp_shelf"));
            var searchPoint = ctx.World.LocationStates["location_store"].SearchPointStates["sp_shelf"];
            Assert.IsTrue(searchPoint.Rolled);
            Assert.AreEqual(3, searchPoint.Inventory.Items["sp_shelf_item_water_0"].Quantity);

            processor.Submit(new TransferItemCommand("searchpoint:sp_shelf", "sp_shelf_item_water_0", "player", 1));
            processor.Submit(new OpenSearchPointCommand("player", "sp_shelf")); // re-open: must not re-roll

            Assert.AreEqual(2, searchPoint.Inventory.Items["sp_shelf_item_water_0"].Quantity);
        }

        [Test]
        public void TakeSome_ThenSaveLoad_RemainingPersists_NoReRoll()
        {
            var ctx = BuildContext();
            var processor = new CommandProcessor(ctx);

            processor.Submit(new OpenSearchPointCommand("player", "sp_shelf"));
            processor.Submit(new TransferItemCommand("searchpoint:sp_shelf", "sp_shelf_item_water_0", "player", 1));

            string json = WorldStateSerializer.SerializeCanonical(ctx.World);
            WorldState restored = WorldStateSerializer.Deserialize(json);

            var restoredSearchPoint = restored.LocationStates["location_store"].SearchPointStates["sp_shelf"];
            Assert.IsTrue(restoredSearchPoint.Rolled);
            Assert.AreEqual(2, restoredSearchPoint.Inventory.Items["sp_shelf_item_water_0"].Quantity);
            Assert.AreEqual(1, restored.Player.Inventory.Items.Count);
        }

        [Test]
        public void TakeAll_EmptiesContainer()
        {
            var ctx = BuildContext();
            var processor = new CommandProcessor(ctx);
            processor.Submit(new OpenSearchPointCommand("player", "sp_shelf"));

            var searchPoint = ctx.World.LocationStates["location_store"].SearchPointStates["sp_shelf"];
            foreach (string instanceId in new List<string>(searchPoint.Inventory.Items.Keys))
            {
                var item = searchPoint.Inventory.Items[instanceId];
                processor.Submit(new TransferItemCommand("searchpoint:sp_shelf", instanceId, "player", item.Quantity));
            }

            Assert.AreEqual(0, searchPoint.Inventory.Items.Count);
            Assert.AreEqual(3, ctx.World.Player.Inventory.Items["sp_shelf_item_water_0"].Quantity);
        }
    }
}
