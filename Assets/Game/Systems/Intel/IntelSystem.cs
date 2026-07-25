using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Intel
{
    /// <summary>
    /// Writes IntelState from direct player observation (S15, npc-framework/world-map intel):
    /// completing a route travel confirms that route and the destination location; opening a
    /// search point confirms its location. Radio/comms and NPC-reported intel (Uncertain/
    /// Reliable without direct observation) arrive with S16+ once a source for them exists.
    /// Recording methods are static so WorldMapPanel can also record a "standing right next to
    /// it" fresh observation for routes connected to the player's current location without
    /// needing its own IntelSystem reference.
    /// </summary>
    public sealed class IntelSystem
    {
        private readonly GameContext _ctx;

        public IntelSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Events.Subscribe<TravelCompleted>(OnTravelCompleted);
            ctx.Events.Subscribe<SearchPointOpened>(OnSearchPointOpened);
        }

        private void OnTravelCompleted(TravelCompleted evt)
        {
            RecordRouteObservation(_ctx, evt.RouteId, IntelConfidence.Confirmed);
            RecordLocationObservation(_ctx, evt.ToLocationId, IntelConfidence.Confirmed);
        }

        private void OnSearchPointOpened(SearchPointOpened evt)
        {
            if (!_ctx.Definitions.TryGetSearchPoint(evt.SearchPointId, out var def)) return;
            RecordLocationObservation(_ctx, def.LocationId, IntelConfidence.Confirmed);
        }

        public static void RecordRouteObservation(GameContext ctx, string routeId, IntelConfidence confidence)
        {
            if (!ctx.Definitions.TryGetRoute(routeId, out RouteDefinition route)) return;

            var hazard = HazardRules.EvaluateRoute(route, ctx.Definitions.DisasterPhasesSorted, ctx.World.WorldTimeMinutes);
            bool closed = hazard.FloodLevel >= HazardRules.MaxLevel || hazard.CurrentLevel >= HazardRules.MaxLevel;
            Record(ctx, new IntelRecord
            {
                SubjectId = routeId,
                Kind = "route",
                Confidence = confidence,
                ObservedAtMinute = ctx.World.WorldTimeMinutes,
                FloodLevel = hazard.FloodLevel,
                CurrentLevel = hazard.CurrentLevel,
                Closed = closed,
            });
        }

        public static void RecordLocationObservation(GameContext ctx, string locationId, IntelConfidence confidence)
        {
            Record(ctx, new IntelRecord
            {
                SubjectId = locationId,
                Kind = "location",
                Confidence = confidence,
                ObservedAtMinute = ctx.World.WorldTimeMinutes,
            });
        }

        private static void Record(GameContext ctx, IntelRecord incoming)
        {
            ctx.World.Intel.Records.TryGetValue(incoming.SubjectId, out var existing);
            if (!IntelRules.ShouldReplace(existing, incoming)) return;

            ctx.World.Intel.Records[incoming.SubjectId] = incoming;
            ctx.Events.Publish(new IntelUpdated(incoming.SubjectId));
        }
    }
}
