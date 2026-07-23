using System.Collections.Generic;
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

        public DefinitionRegistry(
            string definitionVersion,
            BalanceConfig balance,
            Dictionary<string, ItemDefinition> items,
            Dictionary<string, LocationDefinition> locations,
            Dictionary<string, RouteDefinition> routes,
            Dictionary<string, SearchPointDefinition> searchPoints)
        {
            DefinitionVersion = definitionVersion;
            Balance = balance ?? new BalanceConfig();
            Items = items;
            Locations = locations;
            Routes = routes;
            SearchPoints = searchPoints;
        }

        public bool TryGetItem(string id, out ItemDefinition def) => Items.TryGetValue(id, out def);
        public bool TryGetLocation(string id, out LocationDefinition def) => Locations.TryGetValue(id, out def);
        public bool TryGetRoute(string id, out RouteDefinition def) => Routes.TryGetValue(id, out def);
        public bool TryGetSearchPoint(string id, out SearchPointDefinition def) => SearchPoints.TryGetValue(id, out def);
    }
}
