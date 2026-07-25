using System.Collections.Generic;
using System.Linq;
using LastHope.Data.Definitions;

namespace LastHope.Data
{
    /// <summary>
    /// Immutable typed lookup of all Definitions loaded at boot (technical-specification.md mục 9).
    /// Built only by DefinitionLoader.
    /// </summary>
    public sealed class DefinitionRegistry
    {
        public string DefinitionVersion { get; }
        public BalanceConfig Balance { get; }
        public IReadOnlyDictionary<string, ItemDefinition> Items { get; }
        public IReadOnlyDictionary<string, LocationDefinition> Locations { get; }
        public IReadOnlyDictionary<string, RouteDefinition> Routes { get; }
        public IReadOnlyDictionary<string, SearchPointDefinition> SearchPoints { get; }
        public IReadOnlyDictionary<string, DisasterPhaseDefinition> DisasterPhases { get; }
        public IReadOnlyDictionary<string, ShelterZoneDefinition> ShelterZones { get; }
        public IReadOnlyDictionary<string, ModuleDefinition> Modules { get; }
        public IReadOnlyDictionary<string, EventDefinition> Events { get; }
        public IReadOnlyDictionary<string, NpcDefinition> Npcs { get; }

        /// <summary>DisasterPhases ordered by StartMinute, computed once here so
        /// DisasterPhaseSystem/HazardSystem/ReturnWindowCalculator/BeginTravelCommand all walk the
        /// exact same sequence — never re-sorted independently, never drifts.</summary>
        public IReadOnlyList<DisasterPhaseDefinition> DisasterPhasesSorted { get; }

        public DefinitionRegistry(
            string definitionVersion,
            BalanceConfig balance,
            Dictionary<string, ItemDefinition> items,
            Dictionary<string, LocationDefinition> locations,
            Dictionary<string, RouteDefinition> routes,
            Dictionary<string, SearchPointDefinition> searchPoints,
            Dictionary<string, DisasterPhaseDefinition> disasterPhases = null,
            Dictionary<string, ShelterZoneDefinition> shelterZones = null,
            Dictionary<string, ModuleDefinition> modules = null,
            Dictionary<string, EventDefinition> events = null,
            Dictionary<string, NpcDefinition> npcs = null)
        {
            DefinitionVersion = definitionVersion;
            Balance = balance ?? new BalanceConfig();
            Items = items;
            Locations = locations;
            Routes = routes;
            SearchPoints = searchPoints;
            DisasterPhases = disasterPhases ?? new Dictionary<string, DisasterPhaseDefinition>();
            DisasterPhasesSorted = DisasterPhases.Values.OrderBy(p => p.StartMinute).ToList();
            ShelterZones = shelterZones ?? new Dictionary<string, ShelterZoneDefinition>();
            Modules = modules ?? new Dictionary<string, ModuleDefinition>();
            Events = events ?? new Dictionary<string, EventDefinition>();
            Npcs = npcs ?? new Dictionary<string, NpcDefinition>();
        }

        public bool TryGetItem(string id, out ItemDefinition def) => Items.TryGetValue(id, out def);
        public bool TryGetLocation(string id, out LocationDefinition def) => Locations.TryGetValue(id, out def);
        public bool TryGetRoute(string id, out RouteDefinition def) => Routes.TryGetValue(id, out def);
        public bool TryGetSearchPoint(string id, out SearchPointDefinition def) => SearchPoints.TryGetValue(id, out def);
        public bool TryGetDisasterPhase(string id, out DisasterPhaseDefinition def) => DisasterPhases.TryGetValue(id, out def);
        public bool TryGetShelterZone(string id, out ShelterZoneDefinition def) => ShelterZones.TryGetValue(id, out def);
        public bool TryGetModule(string id, out ModuleDefinition def) => Modules.TryGetValue(id, out def);
        public bool TryGetEvent(string id, out EventDefinition def) => Events.TryGetValue(id, out def);
        public bool TryGetNpc(string id, out NpcDefinition def) => Npcs.TryGetValue(id, out def);
    }
}
