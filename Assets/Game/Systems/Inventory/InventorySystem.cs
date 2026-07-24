using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;

namespace LastHope.Systems.Inventory
{
    /// <summary>
    /// Keeps InventoryState.Overload in sync with capacity rules whenever inventory changes
    /// (capacity/overload is a Systems concern — Core state and commands stay rule-agnostic).
    /// </summary>
    public sealed class InventorySystem
    {
        private readonly GameContext _ctx;

        public InventorySystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Events.Subscribe<InventoryChanged>(OnInventoryChanged);
        }

        /// <summary>Call once after boot/load to seed Overload from whatever was restored.</summary>
        public void RecomputeAll()
        {
            Recompute(_ctx.World.Player.Inventory);
        }

        private void OnInventoryChanged(InventoryChanged evt)
        {
            if (evt.OwnerId == _ctx.World.Player.ActorId)
                Recompute(_ctx.World.Player.Inventory);
        }

        private void Recompute(InventoryState inventory)
        {
            OverloadState newOverload = InventoryRules.ComputeOverload(inventory, _ctx.Definitions.Balance, _ctx.Definitions);
            if (newOverload == inventory.Overload) return;

            inventory.Overload = newOverload;
            _ctx.Events.Publish(new OverloadStateChanged(inventory.OwnerId, newOverload));
        }
    }
}
