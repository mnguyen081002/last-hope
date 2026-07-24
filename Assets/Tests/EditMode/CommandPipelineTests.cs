using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.Rules;
using LastHope.Core.Save;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class CommandPipelineTests
    {
        private static GameContext BuildContext()
        {
            var world = new WorldState();
            var bus = new EventBus();
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_test"] = new ItemDefinition
                {
                    Id = "item_test",
                    BaseWeightKg = 1f,
                    BaseVolumeLiters = 1f,
                    MaxStackSize = 10,
                },
                ["item_water"] = new ItemDefinition
                {
                    Id = "item_water",
                    BaseWeightKg = 1f,
                    BaseVolumeLiters = 1f,
                    MaxStackSize = 10,
                    UseEffects = new Dictionary<string, float> { ["thirst"] = -40f },
                },
                ["item_medkit"] = new ItemDefinition
                {
                    Id = "item_medkit",
                    BaseWeightKg = 1f,
                    BaseVolumeLiters = 1f,
                    MaxStackSize = 10,
                    UseEffects = new Dictionary<string, float> { ["health"] = 50f },
                    Tags = new List<string> { "medical" },
                },
                ["item_gloves"] = new ItemDefinition
                {
                    Id = "item_gloves",
                    BaseWeightKg = 0.2f,
                    BaseVolumeLiters = 0.3f,
                    MaxStackSize = 1,
                    EquipSlot = "hands",
                    Protection = new Dictionary<string, float> { ["handles_contaminated"] = 1f },
                },
                ["item_dry_bag"] = new ItemDefinition
                {
                    Id = "item_dry_bag",
                    BaseWeightKg = 2f,
                    BaseVolumeLiters = 3f,
                    MaxStackSize = 1,
                    EquipSlot = "back",
                    Protection = new Dictionary<string, float> { ["backpack_capacity_kg"] = 10f, ["backpack_capacity_liters"] = 18f },
                },
            };
            var registry = new DefinitionRegistry(
                "test",
                new BalanceConfig(),
                items,
                new Dictionary<string, LocationDefinition>(),
                new Dictionary<string, RouteDefinition>(),
                new Dictionary<string, SearchPointDefinition>());
            var rng = new RngService(world);
            var scheduler = new TickScheduler(world, bus);
            return new GameContext(world, registry, bus, rng, scheduler);
        }

        [Test]
        public void TransferItem_InvalidItem_FailsValidation_NoMutation()
        {
            var ctx = BuildContext();
            string beforeJson = WorldStateSerializer.SerializeCanonical(ctx.World);

            var processor = new CommandProcessor(ctx);
            var cmd = new TransferItemCommand(
                ctx.World.Player.ActorId, "item_instance_missing", ctx.World.Player.ActorId, 1);
            var result = processor.Submit(cmd);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.ItemNotFound, result.Code);

            string afterJson = WorldStateSerializer.SerializeCanonical(ctx.World);
            Assert.AreEqual(beforeJson, afterJson);
        }

        [Test]
        public void UseItem_Executes_PublishesInventoryChanged_AndDecrementsQuantity()
        {
            var ctx = BuildContext();
            var item = InventoryOps.AddItem(
                ctx.World.Player.Inventory, ctx.Definitions, "item_test", 3, () => "instance_1");

            InventoryChanged? received = null;
            ctx.Events.Subscribe<InventoryChanged>(e => received = e);

            var processor = new CommandProcessor(ctx);
            var result = processor.Submit(new UseItemCommand(ctx.World.Player.ActorId, item.InstanceId, 1));

            Assert.IsTrue(result.Success);
            Assert.IsTrue(received.HasValue);
            Assert.AreEqual(ctx.World.Player.ActorId, received.Value.OwnerId);
            Assert.AreEqual(2, ctx.World.Player.Inventory.Items[item.InstanceId].Quantity);
        }

        [Test]
        public void UseItem_AppliesUseEffects_ReducesThirst()
        {
            var ctx = BuildContext();
            ctx.World.Player.Condition.Thirst = 50f;
            var item = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_water", 1, () => "water_1");

            var result = new CommandProcessor(ctx).Submit(new UseItemCommand(ctx.World.Player.ActorId, item.InstanceId, 1));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(10f, ctx.World.Player.Condition.Thirst, 0.001f);
        }

        [Test]
        public void UseItem_WhenIncapacitated_NonMedicalItem_IsBlocked()
        {
            var ctx = BuildContext();
            ctx.World.Player.Condition.Health = 3f;
            ConditionOps.RecomputeIncapacitation(ctx.World.Player.Condition, ctx.Definitions.Balance.Condition);
            var item = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_water", 1, () => "water_1");

            var result = new CommandProcessor(ctx).Submit(new UseItemCommand(ctx.World.Player.ActorId, item.InstanceId, 1));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.Incapacitated, result.Code);
        }

        [Test]
        public void UseItem_WhenIncapacitated_MedicalItem_IsAllowed_AndClearsIncapacitation()
        {
            var ctx = BuildContext();
            ctx.World.Player.Condition.Health = 3f;
            ConditionOps.RecomputeIncapacitation(ctx.World.Player.Condition, ctx.Definitions.Balance.Condition);
            var item = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_medkit", 1, () => "medkit_1");

            var result = new CommandProcessor(ctx).Submit(new UseItemCommand(ctx.World.Player.ActorId, item.InstanceId, 1));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(53f, ctx.World.Player.Condition.Health, 0.001f);
            Assert.IsFalse(ConditionOps.IsIncapacitated(ctx.World.Player.Condition));
        }

        [Test]
        public void EquipItem_CorrectSlot_Succeeds_AndPublishesEquipmentChanged()
        {
            var ctx = BuildContext();
            var item = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_gloves", 1, () => "gloves_1");

            EquipmentChanged? received = null;
            ctx.Events.Subscribe<EquipmentChanged>(e => received = e);

            var result = new CommandProcessor(ctx).Submit(new EquipItemCommand(ctx.World.Player.ActorId, item.InstanceId, "hands"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual("gloves_1", ctx.World.Player.Inventory.EquipmentSlots["hands"]);
            Assert.IsTrue(received.HasValue);
            Assert.AreEqual("hands", received.Value.Slot);
        }

        [Test]
        public void EquipItem_WrongSlot_FailsSlotMismatch()
        {
            var ctx = BuildContext();
            var item = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_gloves", 1, () => "gloves_1");

            var result = new CommandProcessor(ctx).Submit(new EquipItemCommand(ctx.World.Player.ActorId, item.InstanceId, "feet"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.SlotMismatch, result.Code);
        }

        [Test]
        public void EquipItem_NonEquippableItem_FailsInvalidTarget()
        {
            var ctx = BuildContext();
            var item = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_test", 1, () => "test_1");

            var result = new CommandProcessor(ctx).Submit(new EquipItemCommand(ctx.World.Player.ActorId, item.InstanceId, "body"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.InvalidTarget, result.Code);
        }

        [Test]
        public void UnequipItem_RemovesSlot_AndPublishesEquipmentChanged()
        {
            var ctx = BuildContext();
            var item = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_gloves", 1, () => "gloves_1");
            new CommandProcessor(ctx).Submit(new EquipItemCommand(ctx.World.Player.ActorId, item.InstanceId, "hands"));

            EquipmentChanged? received = null;
            ctx.Events.Subscribe<EquipmentChanged>(e => received = e);
            var result = new CommandProcessor(ctx).Submit(new UnequipItemCommand(ctx.World.Player.ActorId, "hands"));

            Assert.IsTrue(result.Success);
            Assert.IsFalse(ctx.World.Player.Inventory.EquipmentSlots.ContainsKey("hands"));
            Assert.IsTrue(received.HasValue);
            Assert.IsNull(received.Value.ItemInstanceId);
        }

        [Test]
        public void UnequipItem_EmptySlot_FailsSlotMismatch()
        {
            var ctx = BuildContext();

            var result = new CommandProcessor(ctx).Submit(new UnequipItemCommand(ctx.World.Player.ActorId, "hands"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.SlotMismatch, result.Code);
        }

        [Test]
        public void DryBagEquipped_OverridesBackpackCapacity()
        {
            var ctx = BuildContext();
            var bag = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_dry_bag", 1, () => "bag_1");
            new CommandProcessor(ctx).Submit(new EquipItemCommand(ctx.World.Player.ActorId, bag.InstanceId, "back"));

            var (weightCap, volumeCap) = InventoryRules.EffectiveCapacity(ctx.World.Player.Inventory, ctx.Definitions, ctx.Definitions.Balance);

            Assert.AreEqual(10f, weightCap);
            Assert.AreEqual(18f, volumeCap);
        }

        [Test]
        public void TransferItem_ContaminatedToPlayer_WithoutGloves_AddsExposure()
        {
            var ctx = BuildContext();
            var dropped = new InventoryState { OwnerId = "location_dropped:loc" };
            ctx.World.LocationStates["loc"] = new LocationState { Id = "loc", DroppedItems = dropped };
            dropped.Items["dirty_1"] = new ItemInstanceState { InstanceId = "dirty_1", ItemId = "item_test", Quantity = 1, Contamination = ContaminationState.Contaminated };

            var result = new CommandProcessor(ctx).Submit(
                new TransferItemCommand("location_dropped:loc", "dirty_1", ctx.World.Player.ActorId, 1));

            Assert.IsTrue(result.Success);
            Assert.Greater(ConditionOps.GetExposure(ctx.World.Player.Condition, "black_water"), 0f);
        }

        [Test]
        public void TransferItem_ContaminatedToPlayer_WithGloves_NoExposure()
        {
            var ctx = BuildContext();
            var gloves = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_gloves", 1, () => "gloves_1");
            new CommandProcessor(ctx).Submit(new EquipItemCommand(ctx.World.Player.ActorId, gloves.InstanceId, "hands"));

            var dropped = new InventoryState { OwnerId = "location_dropped:loc" };
            ctx.World.LocationStates["loc"] = new LocationState { Id = "loc", DroppedItems = dropped };
            dropped.Items["dirty_1"] = new ItemInstanceState { InstanceId = "dirty_1", ItemId = "item_test", Quantity = 1, Contamination = ContaminationState.Contaminated };

            var result = new CommandProcessor(ctx).Submit(
                new TransferItemCommand("location_dropped:loc", "dirty_1", ctx.World.Player.ActorId, 1));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0f, ConditionOps.GetExposure(ctx.World.Player.Condition, "black_water"));
        }
    }
}
