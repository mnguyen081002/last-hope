using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Condition;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class RestAtShelterCommandTests
    {
        private static GameContext BuildContext(bool atShelter, Dictionary<string, ItemDefinition> items = null)
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = atShelter ? "loc_shelter" : "loc_outside";
            var bus = new EventBus();

            var locations = new Dictionary<string, LocationDefinition>
            {
                ["loc_shelter"] = new LocationDefinition { Id = "loc_shelter", IsShelter = true },
                ["loc_outside"] = new LocationDefinition { Id = "loc_outside", IsShelter = false },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), items ?? new Dictionary<string, ItemDefinition>(),
                locations, new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>());
            var scheduler = new TickScheduler(world, bus);
            return new GameContext(world, registry, bus, new RngService(world), scheduler);
        }

        [Test]
        public void NotAtShelter_FailsNotAtLocation()
        {
            var ctx = BuildContext(atShelter: false);

            var result = new CommandProcessor(ctx).Submit(new RestAtShelterCommand("player", RestMode.Rest));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NotAtLocation, result.Code);
        }

        [Test]
        public void Rest_AtShelter_FastForwardsClock()
        {
            var ctx = BuildContext(atShelter: true);
            long before = ctx.World.WorldTimeMinutes;

            var result = new CommandProcessor(ctx).Submit(new RestAtShelterCommand("player", RestMode.Rest));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(before + ctx.Definitions.Balance.Condition.ShelterRestMinutes, ctx.World.WorldTimeMinutes);
        }

        [Test]
        public void TreatExposure_NoMedicalItem_FailsNoMedicalItem()
        {
            var ctx = BuildContext(atShelter: true);

            var result = new CommandProcessor(ctx).Submit(new RestAtShelterCommand("player", RestMode.TreatExposure));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NoMedicalItem, result.Code);
        }

        [Test]
        public void TreatExposure_ConsumesMedkit_AppliesHealEffect_AndDecaysExposure()
        {
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_medkit"] = new ItemDefinition
                {
                    Id = "item_medkit",
                    Tags = new List<string> { "medical" },
                    UseEffects = new Dictionary<string, float> { ["health"] = 20f },
                },
            };
            var ctx = BuildContext(atShelter: true, items: items);
            _ = new ConditionSystem(ctx); // needed so LongTick applies the TreatingExposure decay during FastForward
            var condition = ctx.World.Player.Condition;
            condition.Health = 50f;
            ConditionOps.AddExposure(condition, "black_water", 30f);
            var medkit = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_medkit", 1, () => "medkit_1");

            var result = new CommandProcessor(ctx).Submit(new RestAtShelterCommand("player", RestMode.TreatExposure));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(70f, condition.Health, 0.001f); // 50 + 20 heal
            Assert.IsFalse(ctx.World.Player.Inventory.Items.ContainsKey(medkit.InstanceId)); // consumed
            Assert.IsFalse(condition.TreatingExposure); // cleared after the session ends

            // 60 minutes = 6 long ticks, each -5 exposure => 30 - 30 = 0
            Assert.AreEqual(0f, ConditionOps.GetExposure(condition, "black_water"), 0.001f);
        }

        [Test]
        public void DryOff_ClearsWetStatus_WithoutAdvancingClock()
        {
            var ctx = BuildContext(atShelter: true);
            var condition = ctx.World.Player.Condition;
            ConditionOps.SetStatusSeverity(condition, ConditionOps.StatusWet, 80f, 0);
            long before = ctx.World.WorldTimeMinutes;

            var result = new CommandProcessor(ctx).Submit(new RestAtShelterCommand("player", RestMode.DryOff));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(condition, ConditionOps.StatusWet));
            Assert.AreEqual(before, ctx.World.WorldTimeMinutes);
        }
    }
}
