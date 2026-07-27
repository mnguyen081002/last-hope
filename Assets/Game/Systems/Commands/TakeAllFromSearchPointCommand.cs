using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;
using LastHope.Systems.Search;

namespace LastHope.Systems.Commands
{
    /// <summary>Lấy hết mọi thứ còn trong search point, giới hạn bởi sức chứa player.</summary>
    public class TakeAllFromSearchPointCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string SearchPointId;

        /// <summary>False sau Execute nghĩa là còn sót lại đồ (triage) — đọc sau khi Submit.</summary>
        public bool TookEverything { get; private set; }

        public TakeAllFromSearchPointCommand(string searchPointId) => SearchPointId = searchPointId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetSearchPoint(SearchPointId, out var definition))
                return CommandResult.Fail(CommandErrorCode.UnknownDefinition, SearchPointId);

            var location = context.World.GetOrCreateLocation(definition.LocationId);
            if (!location.SearchPoints.TryGetValue(SearchPointId, out var state) || !state.Rolled)
                return CommandResult.Fail(CommandErrorCode.NotAllowedNow, "Search point chưa mở.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            TookEverything = SearchSystem.TakeAll(context.World, context.Definitions, SearchPointId);
            context.Events?.Publish(new InventoryChanged(InventoryOwner.Player.ToString()));
        }
    }
}
