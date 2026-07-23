using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Data.Definitions;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Opens a search point container (BL-P1-17, decision 2026-07-24: "see everything, take
    /// everything, decide at carry capacity"). Rolls its loot table ONCE on first open, deterministic
    /// via the "loot" RNG stream, then leaves whatever isn't taken in place forever — nothing is
    /// re-rolled, nothing disappears until a player actually takes it.
    /// </summary>
    public sealed class OpenSearchPointCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // search point id
        public long WorldTime { get; set; }

        public OpenSearchPointCommand(string actorId, string searchPointId)
        {
            ActorId = actorId;
            TargetId = searchPointId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (ActorId != ctx.World.Player.ActorId)
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown actor '{ActorId}'.");

            if (!ctx.Definitions.TryGetSearchPoint(TargetId, out var def))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"Unknown search point '{TargetId}'.");

            if (ctx.World.Player.CurrentLocationId != def.LocationId)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, $"Player is not at '{def.LocationId}'.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.Definitions.TryGetSearchPoint(TargetId, out SearchPointDefinition def);

            if (!ctx.World.LocationStates.TryGetValue(def.LocationId, out var location))
            {
                location = new LocationState { Id = def.LocationId };
                ctx.World.LocationStates[def.LocationId] = location;
            }

            if (!location.SearchPointStates.TryGetValue(TargetId, out var searchPoint))
            {
                searchPoint = new SearchPointState
                {
                    SearchPointId = TargetId,
                    Inventory = new InventoryState { OwnerId = "searchpoint:" + TargetId },
                };
                location.SearchPointStates[TargetId] = searchPoint;
            }

            bool firstOpen = !searchPoint.Rolled;
            if (firstOpen)
            {
                RollLoot(ctx, def, searchPoint.Inventory);
                searchPoint.Rolled = true;

                if (def.OpenTimeMinutes > 0) ctx.Clock.FastForward(def.OpenTimeMinutes);
            }

            ctx.Events.Publish(new SearchPointOpened(TargetId, firstOpen));
            ctx.Events.Publish(new ContainerViewRequested("searchpoint:" + TargetId, def.DisplayNameKey ?? TargetId));
        }

        private static void RollLoot(GameContext ctx, SearchPointDefinition def, InventoryState inventory)
        {
            var loot = ctx.Rng.GetStream("loot");
            int idCounter = 0;

            foreach (var entry in def.LootTable)
            {
                if (entry.Weight <= 0) continue;

                int quantity = entry.MinQuantity == entry.MaxQuantity
                    ? entry.MinQuantity
                    : loot.NextInt(entry.MinQuantity, entry.MaxQuantity + 1);
                if (quantity <= 0) continue;

                InventoryOps.AddItem(inventory, ctx.Definitions, entry.ItemId, quantity,
                    () => $"{def.Id}_{entry.ItemId}_{idCounter++}");
            }
        }
    }
}
