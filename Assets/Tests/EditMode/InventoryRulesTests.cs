using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Inventory;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class InventoryRulesTests
    {
        private static DefinitionRegistry BuildRegistry()
        {
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_test"] = new ItemDefinition { Id = "item_test", BaseWeightKg = 1f, BaseVolumeLiters = 1f, MaxStackSize = 100 }
            };
            return new DefinitionRegistry(
                "test", new BalanceConfig(), items,
                new Dictionary<string, LocationDefinition>(),
                new Dictionary<string, RouteDefinition>(),
                new Dictionary<string, SearchPointDefinition>());
        }

        private static GameContext BuildContext(out InventorySystem inventorySystem)
        {
            var world = new WorldState();
            var bus = new EventBus();
            var ctx = new GameContext(world, BuildRegistry(), bus, new RngService(world), new TickScheduler(world, bus));
            inventorySystem = new InventorySystem(ctx);
            return ctx;
        }

        [Test]
        public void UnderThreshold_IsNormal()
        {
            var inv = new InventoryState { OwnerId = "player", CurrentWeightKg = 14f, CurrentVolumeLiters = 24f };
            Assert.AreEqual(OverloadState.Normal, InventoryRules.ComputeOverload(inv, new BalanceConfig()));
        }

        [Test]
        public void JustOverWeightCap_IsLight()
        {
            var inv = new InventoryState { OwnerId = "player", CurrentWeightKg = 15.1f, CurrentVolumeLiters = 5f };
            Assert.AreEqual(OverloadState.Light, InventoryRules.ComputeOverload(inv, new BalanceConfig()));
        }

        [Test]
        public void FarOverWeightCap_IsHeavy()
        {
            var inv = new InventoryState { OwnerId = "player", CurrentWeightKg = 19.6f, CurrentVolumeLiters = 5f };
            Assert.AreEqual(OverloadState.Heavy, InventoryRules.ComputeOverload(inv, new BalanceConfig()));
        }

        [Test]
        public void VolumeOnlyOverflow_AlsoTripsOverload()
        {
            // Weight is fine (5kg), volume alone exceeds the light threshold (25L * 1.0 = 25L).
            var inv = new InventoryState { OwnerId = "player", CurrentWeightKg = 5f, CurrentVolumeLiters = 26f };
            Assert.AreEqual(OverloadState.Light, InventoryRules.ComputeOverload(inv, new BalanceConfig()));
        }

        [Test]
        public void CanAccept_PastHardCap_Rejected()
        {
            var registry = BuildRegistry();
            var inv = new InventoryState { OwnerId = "player", CurrentWeightKg = 22f, CurrentVolumeLiters = 0f };
            // 22 + 1*1 = 23 > 15 * 1.5 = 22.5 -> rejected
            Assert.IsFalse(InventoryRules.CanAccept(inv, registry, registry.Balance, "item_test", 1));
        }

        [Test]
        public void CanAccept_NonCapacityLimitedOwner_AlwaysAccepts()
        {
            var registry = BuildRegistry();
            var inv = new InventoryState { OwnerId = "searchpoint:x", CurrentWeightKg = 1000f };
            Assert.IsTrue(InventoryRules.CanAccept(inv, registry, registry.Balance, "item_test", 999));
        }

        [Test]
        public void InventorySystem_PublishesOverloadStateChanged_OnceOnTransition()
        {
            var ctx = BuildContext(out _);
            int publishCount = 0;
            OverloadState lastState = OverloadState.Normal;
            ctx.Events.Subscribe<OverloadStateChanged>(e => { publishCount++; lastState = e.Overload; });

            var inv = ctx.World.Player.Inventory;
            inv.CurrentWeightKg = 16f; // Light
            ctx.Events.Publish(new InventoryChanged("player"));

            Assert.AreEqual(1, publishCount);
            Assert.AreEqual(OverloadState.Light, lastState);
            Assert.AreEqual(OverloadState.Light, inv.Overload);

            // Publishing again with the same weight must NOT re-publish (no state change).
            ctx.Events.Publish(new InventoryChanged("player"));
            Assert.AreEqual(1, publishCount);
        }

        [Test]
        public void InventorySystem_ShrinkingBack_PublishesTransitionDown()
        {
            var ctx = BuildContext(out _);
            var transitions = new List<OverloadState>();
            ctx.Events.Subscribe<OverloadStateChanged>(e => transitions.Add(e.Overload));

            var inv = ctx.World.Player.Inventory;
            inv.CurrentWeightKg = 19.6f; // Heavy
            ctx.Events.Publish(new InventoryChanged("player"));
            inv.CurrentWeightKg = 5f; // back to Normal
            ctx.Events.Publish(new InventoryChanged("player"));

            CollectionAssert.AreEqual(new[] { OverloadState.Heavy, OverloadState.Normal }, transitions);
        }
    }
}
