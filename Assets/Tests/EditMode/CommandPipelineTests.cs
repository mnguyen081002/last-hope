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
                }
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
    }
}
