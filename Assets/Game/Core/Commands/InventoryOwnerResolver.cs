using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Resolves an owner id to its InventoryState. Only the player is a known owner in Sprint 3 —
    /// NPC/Shelter storage owners are added as those systems land (Sprint 5/S6+), by extending
    /// this resolver, not by changing command signatures (commands stay plain ids).
    /// </summary>
    internal static class InventoryOwnerResolver
    {
        public static bool TryResolve(GameContext ctx, string ownerId, out InventoryState inventory)
        {
            if (ownerId == ctx.World.Player.ActorId)
            {
                inventory = ctx.World.Player.Inventory;
                return true;
            }
            inventory = null;
            return false;
        }
    }
}
