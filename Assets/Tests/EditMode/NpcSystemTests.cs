using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Npc;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class NpcSystemTests
    {
        private const string Shelter = "location_shelter";

        private static GameContext BuildContext(NpcBalance npcCfg = null)
        {
            var world = new WorldState();
            var balance = new BalanceConfig();
            if (npcCfg != null) balance.Npc = npcCfg;

            var bus = new EventBus();
            var locations = new Dictionary<string, LocationDefinition>
            {
                [Shelter] = new LocationDefinition { Id = Shelter, IsShelter = true },
            };
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_canned_food"] = new ItemDefinition { Id = "item_canned_food", Category = "food" },
            };
            var registry = new DefinitionRegistry(
                "test", balance, items, locations,
                new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>());
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);

            world.ShelterStates["shelter_main"] = new ShelterState
            {
                Id = "shelter_main",
                Storage = new InventoryState { OwnerId = "shelter_storage:shelter_main" },
            };

            _ = new NpcSystem(ctx);
            return ctx;
        }

        private static NpcState Recruit(GameContext ctx, string id = "npc_minh", int trust = 30)
        {
            var npc = new NpcState { Id = id, Recruited = true, LocationId = Shelter, Trust = trust };
            ctx.World.NpcStates[id] = npc;
            return npc;
        }

        [Test]
        public void LongTick_AccruesHungerAndThirst()
        {
            var cfg = new NpcBalance { ThirstPerLongTick = 5f, HungerPerLongTick = 3f };
            var ctx = BuildContext(cfg);
            Recruit(ctx);

            ctx.Clock.FastForward(10);

            var npc = ctx.World.NpcStates["npc_minh"];
            Assert.AreEqual(5f, npc.Thirst);
            Assert.AreEqual(3f, npc.Hunger);
        }

        [Test]
        public void Unrecruited_Npc_DoesNotAccrue()
        {
            var ctx = BuildContext();
            var npc = new NpcState { Id = "npc_minh", Recruited = false, LocationId = Shelter };
            ctx.World.NpcStates["npc_minh"] = npc;

            ctx.Clock.FastForward(10);

            Assert.AreEqual(0f, npc.Thirst);
        }

        [Test]
        public void ThirstMaxed_ConsumesCleanWater_ResetsThirst_GainsTrust()
        {
            var cfg = new NpcBalance { ThirstPerLongTick = 100f, WaterConsumedPerFeed = 2f, TrustGainOnFed = 5 };
            var ctx = BuildContext(cfg);
            var npc = Recruit(ctx, trust: 30);
            ctx.World.ShelterStates["shelter_main"].WaterStocks.Clean = 10f;

            ctx.Clock.FastForward(10);

            Assert.AreEqual(0f, npc.Thirst);
            Assert.AreEqual(8f, ctx.World.ShelterStates["shelter_main"].WaterStocks.Clean);
            Assert.AreEqual(35, npc.Trust);
        }

        [Test]
        public void ThirstMaxed_NoWater_TrustDrops_StarvingCounterIncrements()
        {
            var cfg = new NpcBalance { ThirstPerLongTick = 100f, WaterConsumedPerFeed = 2f, TrustLossOnHungry = 4, StarvingLongTicksPerHealthDrop = 99 };
            var ctx = BuildContext(cfg);
            var npc = Recruit(ctx, trust: 30);
            ctx.World.ShelterStates["shelter_main"].WaterStocks.Clean = 0f;

            ctx.Clock.FastForward(10);

            Assert.AreEqual(100f, npc.Thirst); // still capped, unmet
            Assert.AreEqual(26, npc.Trust);
            Assert.AreEqual(1, npc.StarvingLongTicks);
        }

        [Test]
        public void HungerMaxed_ConsumesFoodItem_DecrementsStack_RemovesWhenEmpty()
        {
            var cfg = new NpcBalance { HungerPerLongTick = 100f, ThirstPerLongTick = 0f };
            var ctx = BuildContext(cfg);
            var npc = Recruit(ctx);
            ctx.World.ShelterStates["shelter_main"].Storage.Items["inst_food"] = new ItemInstanceState
            {
                InstanceId = "inst_food",
                ItemId = "item_canned_food",
                Quantity = 1,
            };

            ctx.Clock.FastForward(10);

            Assert.AreEqual(0f, npc.Hunger);
            Assert.IsFalse(ctx.World.ShelterStates["shelter_main"].Storage.Items.ContainsKey("inst_food"));
        }

        [Test]
        public void StarvingPastThreshold_DowngradesHealthOneStep_ResetsCounter()
        {
            var cfg = new NpcBalance
            {
                ThirstPerLongTick = 100f, WaterConsumedPerFeed = 2f,
                HungerPerLongTick = 0f, // isolate to thirst shortage only
                StarvingLongTicksPerHealthDrop = 2,
            };
            var ctx = BuildContext(cfg);
            var npc = Recruit(ctx);
            ctx.World.ShelterStates["shelter_main"].WaterStocks.Clean = 0f;

            ctx.Clock.FastForward(10);
            Assert.AreEqual(NpcHealthState.Healthy, npc.Health);
            ctx.Clock.FastForward(10);

            Assert.AreEqual(NpcHealthState.Injured, npc.Health);
            Assert.AreEqual(0, npc.StarvingLongTicks);
        }

        [Test]
        public void StarvingUntilDead_PublishesNpcDied_DecrementsOccupants()
        {
            var cfg = new NpcBalance
            {
                ThirstPerLongTick = 100f, WaterConsumedPerFeed = 2f,
                HungerPerLongTick = 0f,
                StarvingLongTicksPerHealthDrop = 1,
            };
            var ctx = BuildContext(cfg);
            var npc = Recruit(ctx);
            ctx.World.ShelterStates["shelter_main"].WaterStocks.Clean = 0f;
            ctx.World.ShelterStates["shelter_main"].Occupants = 2;
            string diedId = null;
            ctx.Events.Subscribe<NpcDied>(e => diedId = e.NpcId);

            ctx.Clock.FastForward(30); // 3 drops: Healthy->Injured->Critical->Dead

            Assert.AreEqual(NpcHealthState.Dead, npc.Health);
            Assert.AreEqual("npc_minh", diedId);
            Assert.AreEqual(1, ctx.World.ShelterStates["shelter_main"].Occupants);
        }

        [Test]
        public void Dead_Npc_NoLongerTicked()
        {
            var cfg = new NpcBalance { ThirstPerLongTick = 10f };
            var ctx = BuildContext(cfg);
            var npc = Recruit(ctx);
            npc.Health = NpcHealthState.Dead;

            ctx.Clock.FastForward(10);

            Assert.AreEqual(0f, npc.Thirst);
        }

        [Test]
        public void FloodedShelter_PastThreshold_DowngradesHealth()
        {
            var cfg = new NpcBalance { ThirstPerLongTick = 0f, HungerPerLongTick = 0f, FloodLongTicksPerHealthDrop = 2 };
            var ctx = BuildContext(cfg);
            var npc = Recruit(ctx);
            ctx.World.ShelterStates["shelter_main"].WaterIntrusion.Level = WaterIntrusionLevel.Deep;

            ctx.Clock.FastForward(10);
            Assert.AreEqual(NpcHealthState.Healthy, npc.Health);
            ctx.Clock.FastForward(10);

            Assert.AreEqual(NpcHealthState.Injured, npc.Health);
        }

        [Test]
        public void FloodRecovers_ResetsExposureCounter_NoDowngrade()
        {
            var cfg = new NpcBalance { ThirstPerLongTick = 0f, HungerPerLongTick = 0f, FloodLongTicksPerHealthDrop = 2 };
            var ctx = BuildContext(cfg);
            var npc = Recruit(ctx);
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Deep;

            ctx.Clock.FastForward(10);
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Dry;
            ctx.Clock.FastForward(10); // exposure counter resets here
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Deep;
            ctx.Clock.FastForward(10); // only 1 tick of exposure again — below threshold of 2

            Assert.AreEqual(NpcHealthState.Healthy, npc.Health);
        }
    }
}
