using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class OwnerResolverTests
    {
        private static GameContext BuildContext()
        {
            var world = new WorldState { RandomSeed = 1 };
            var bus = new EventBus();
            var searchPoints = new Dictionary<string, SearchPointDefinition>
            {
                ["sp_1"] = new SearchPointDefinition { Id = "sp_1", LocationId = "loc_1", LootTable = new List<LootEntry>() },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                new Dictionary<string, LocationDefinition>(), new Dictionary<string, RouteDefinition>(), searchPoints);
            return new GameContext(world, registry, bus, new RngService(world), new TickScheduler(world, bus));
        }

        [Test]
        public void Player_AlwaysResolves()
        {
            var ctx = BuildContext();
            Assert.IsTrue(InventoryOwnerResolver.TryResolve(ctx, "player", out var inv));
            Assert.AreSame(ctx.World.Player.Inventory, inv);
        }

        [Test]
        public void SearchPoint_Unrolled_Fails()
        {
            var ctx = BuildContext();
            ctx.World.LocationStates["loc_1"] = new LocationState { Id = "loc_1" };
            ctx.World.LocationStates["loc_1"].SearchPointStates["sp_1"] = new SearchPointState
            {
                SearchPointId = "sp_1", Rolled = false, Inventory = new InventoryState { OwnerId = "searchpoint:sp_1" },
            };

            Assert.IsFalse(InventoryOwnerResolver.TryResolve(ctx, "searchpoint:sp_1", out _));
        }

        [Test]
        public void SearchPoint_Rolled_Resolves()
        {
            var ctx = BuildContext();
            var inventory = new InventoryState { OwnerId = "searchpoint:sp_1" };
            ctx.World.LocationStates["loc_1"] = new LocationState { Id = "loc_1" };
            ctx.World.LocationStates["loc_1"].SearchPointStates["sp_1"] = new SearchPointState
            {
                SearchPointId = "sp_1", Rolled = true, Inventory = inventory,
            };

            Assert.IsTrue(InventoryOwnerResolver.TryResolve(ctx, "searchpoint:sp_1", out var resolved));
            Assert.AreSame(inventory, resolved);
        }

        [Test]
        public void ShelterStorage_LazilyCreated()
        {
            var ctx = BuildContext();
            Assert.IsFalse(ctx.World.ShelterStates.ContainsKey("shelter_main"));

            Assert.IsTrue(InventoryOwnerResolver.TryResolve(ctx, "shelter_storage:shelter_main", out var inv));
            Assert.IsNotNull(inv);
            Assert.IsTrue(ctx.World.ShelterStates.ContainsKey("shelter_main"));

            // Second resolve returns the SAME instance, not a fresh one.
            InventoryOwnerResolver.TryResolve(ctx, "shelter_storage:shelter_main", out var inv2);
            Assert.AreSame(inv, inv2);
        }

        [Test]
        public void LocationDropped_LazilyCreated()
        {
            var ctx = BuildContext();
            Assert.IsTrue(InventoryOwnerResolver.TryResolve(ctx, "location_dropped:loc_1", out var inv));
            Assert.IsNotNull(inv);
            Assert.IsTrue(ctx.World.LocationStates["loc_1"].DroppedItems != null);
        }

        [Test]
        public void UnknownPrefix_Fails()
        {
            var ctx = BuildContext();
            Assert.IsFalse(InventoryOwnerResolver.TryResolve(ctx, "npc:someone", out _));
            Assert.IsFalse(InventoryOwnerResolver.TryResolve(ctx, "garbage", out _));
        }
    }
}
