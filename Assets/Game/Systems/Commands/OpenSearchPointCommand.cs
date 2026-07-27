using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Search;

namespace LastHope.Systems.Commands
{
    /// <summary>
    /// Mở search point. Việc "giữ phím đủ lâu" (nếu <c>OpenHoldSeconds &gt; 0</c>) xảy ra
    /// hoàn toàn ở Presentation trước khi command này được submit — command chỉ roll loot
    /// và publish, không biết gì về thời gian thực.
    /// </summary>
    public class OpenSearchPointCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string SearchPointId;

        public OpenSearchPointCommand(string searchPointId) => SearchPointId = searchPointId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetSearchPoint(SearchPointId, out var definition))
                return CommandResult.Fail(CommandErrorCode.UnknownDefinition, SearchPointId);

            if (context.World.Player.CurrentLocationId != definition.LocationId)
                return CommandResult.Fail(CommandErrorCode.WrongLocation, SearchPointId);

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            SearchSystem.Open(context.World, context.Definitions, context.Rng, SearchPointId);
            context.Events?.Publish(new SearchPointOpened(SearchPointId));
        }
    }
}
