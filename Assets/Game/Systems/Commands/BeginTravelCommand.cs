using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Systems.Hazard;
using LastHope.Systems.Travel;

namespace LastHope.Systems.Commands
{
    /// <summary>
    /// Di chuyển giữa hai location nối bởi route. Bơm thời gian qua
    /// <see cref="TravelSystem.Travel"/> rồi publish <see cref="LocationChanged"/> —
    /// Presentation (SceneFlowController) nghe event này để đổi scene, Command không đụng
    /// SceneManager.
    /// </summary>
    public class BeginTravelCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string RouteId;

        public BeginTravelCommand(string routeId) => RouteId = routeId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetRoute(RouteId, out var route))
                return CommandResult.Fail(CommandErrorCode.UnknownDefinition, RouteId);

            if (!route.Connects(context.World.Player.CurrentLocationId))
                return CommandResult.Fail(CommandErrorCode.WrongLocation,
                    $"Route '{RouteId}' không nối location hiện tại.");

            var phase = DisasterPhaseSystem.CurrentPhase(
                context.World.WorldTimeMinutes, context.Definitions.Balance.DisasterPhase);
            var flood = HazardSystem.EffectiveFlood(route, context.World.GetOrCreateRoute(RouteId), phase);
            if (!HazardSystem.IsPassable(flood))
                return CommandResult.Fail(CommandErrorCode.NotAllowedNow,
                    $"Route '{RouteId}' đang Impassable — không đi qua được.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            string fromLocationId = context.World.Player.CurrentLocationId;

            int minutes = TravelSystem.ComputeTravelMinutes(context.World, context.Definitions, RouteId);
            context.Events?.Publish(new TravelStarted(RouteId, minutes));

            var hazardRng = context.Rng.Stream(RngService.Events);
            TravelSystem.Travel(context.World, context.Definitions, context.Ticks, hazardRng, RouteId);

            context.Events?.Publish(new LocationChanged(fromLocationId, context.World.Player.CurrentLocationId));
        }
    }
}
