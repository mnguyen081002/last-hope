using System;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Resolves an owner id to its InventoryState. Owner-id scheme (Sprint 6, BL-P1-17/18):
    ///   "player"                     -> World.Player.Inventory
    ///   "searchpoint:&lt;id&gt;"          -> that search point's rolled loot (never creates state —
    ///                                    must already be opened via OpenSearchPointCommand)
    ///   "shelter_storage:&lt;id&gt;"      -> that shelter's storage (lazily created)
    ///   "location_dropped:&lt;id&gt;"     -> that location's dropped-item pile (lazily created)
    ///   "task:&lt;taskId&gt;"             -> that task's reserved-materials pile (lazily created,
    ///                                    S11 — StartBuildCommand moves materials here for the
    ///                                    task's duration; TaskSystem consumes/CancelTaskCommand
    ///                                    returns them on completion/cancel)
    ///   "npc:&lt;id&gt;"                   -> that NPC's personal inventory (lazily created, S15 —
    ///                                    requires an existing NpcState; NpcState itself is only
    ///                                    created by RecruitNpcCommand, S16)
    /// New owner kinds are added here, not by changing command signatures — commands stay plain
    /// ids. Public (not internal) so UI code can use the same resolution for read-only display
    /// (ContainerPanel) — only commands mutate state.
    /// </summary>
    public static class InventoryOwnerResolver
    {
        private const string SearchPointPrefix = "searchpoint:";
        private const string ShelterStoragePrefix = "shelter_storage:";
        private const string LocationDroppedPrefix = "location_dropped:";
        private const string TaskPrefix = "task:";
        private const string NpcPrefix = "npc:";

        public static bool TryResolve(GameContext ctx, string ownerId, out InventoryState inventory)
        {
            if (ownerId == ctx.World.Player.ActorId)
            {
                inventory = ctx.World.Player.Inventory;
                return true;
            }

            if (ownerId != null)
            {
                if (ownerId.StartsWith(SearchPointPrefix, StringComparison.Ordinal))
                    return TryResolveSearchPoint(ctx, ownerId.Substring(SearchPointPrefix.Length), out inventory);

                if (ownerId.StartsWith(ShelterStoragePrefix, StringComparison.Ordinal))
                    return TryResolveShelterStorage(ctx, ownerId.Substring(ShelterStoragePrefix.Length), out inventory);

                if (ownerId.StartsWith(LocationDroppedPrefix, StringComparison.Ordinal))
                    return TryResolveLocationDropped(ctx, ownerId.Substring(LocationDroppedPrefix.Length), out inventory);

                if (ownerId.StartsWith(TaskPrefix, StringComparison.Ordinal))
                    return TryResolveTask(ctx, ownerId.Substring(TaskPrefix.Length), out inventory);

                if (ownerId.StartsWith(NpcPrefix, StringComparison.Ordinal))
                    return TryResolveNpc(ctx, ownerId.Substring(NpcPrefix.Length), out inventory);
            }

            inventory = null;
            return false;
        }

        private static bool TryResolveSearchPoint(GameContext ctx, string searchPointId, out InventoryState inventory)
        {
            inventory = null;
            if (!ctx.Definitions.TryGetSearchPoint(searchPointId, out var def)) return false;
            if (!ctx.World.LocationStates.TryGetValue(def.LocationId, out var location)) return false;
            if (!location.SearchPointStates.TryGetValue(searchPointId, out var searchPoint) || !searchPoint.Rolled) return false;

            inventory = searchPoint.Inventory;
            return true;
        }

        private static bool TryResolveShelterStorage(GameContext ctx, string shelterId, out InventoryState inventory)
        {
            if (!ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter))
            {
                shelter = new ShelterState { Id = shelterId };
                ctx.World.ShelterStates[shelterId] = shelter;
            }
            shelter.Storage ??= new InventoryState { OwnerId = ShelterStoragePrefix + shelterId };

            inventory = shelter.Storage;
            return true;
        }

        private static bool TryResolveLocationDropped(GameContext ctx, string locationId, out InventoryState inventory)
        {
            if (!ctx.World.LocationStates.TryGetValue(locationId, out var location))
            {
                location = new LocationState { Id = locationId };
                ctx.World.LocationStates[locationId] = location;
            }
            location.DroppedItems ??= new InventoryState { OwnerId = LocationDroppedPrefix + locationId };

            inventory = location.DroppedItems;
            return true;
        }

        private static bool TryResolveTask(GameContext ctx, string taskId, out InventoryState inventory)
        {
            if (!ctx.World.TaskInventories.TryGetValue(taskId, out inventory))
            {
                inventory = new InventoryState { OwnerId = TaskPrefix + taskId };
                ctx.World.TaskInventories[taskId] = inventory;
            }
            return true;
        }

        /// <summary>Unlike the other owner kinds, this does NOT lazily create the NpcState itself
        /// — only RecruitNpcCommand (S16) does that. It lazily creates the Inventory field on an
        /// already-existing NpcState, same as ShelterState.Storage.</summary>
        private static bool TryResolveNpc(GameContext ctx, string npcId, out InventoryState inventory)
        {
            inventory = null;
            if (!ctx.World.NpcStates.TryGetValue(npcId, out var npc)) return false;

            npc.Inventory ??= new InventoryState { OwnerId = NpcPrefix + npcId };
            inventory = npc.Inventory;
            return true;
        }
    }
}
