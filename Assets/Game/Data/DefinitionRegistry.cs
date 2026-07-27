using System.Collections.Generic;
using LastHope.Data.Definitions;

namespace LastHope.Data
{
    /// <summary>
    /// Kho definition đã load, chỉ đọc trong lúc chơi. Gameplay logic tra ở đây thay vì
    /// hard-code ID/số liệu (BL-X-05).
    /// </summary>
    public class DefinitionRegistry
    {
        readonly Dictionary<string, ItemDefinition> items = new();
        readonly Dictionary<string, LocationDefinition> locations = new();
        readonly Dictionary<string, RouteDefinition> routes = new();
        readonly Dictionary<string, SearchPointDefinition> searchPoints = new();

        public string DefinitionVersion { get; internal set; } = "0.0.0";
        public BalanceDefinition Balance { get; internal set; } = new();

        public IReadOnlyDictionary<string, ItemDefinition> Items => items;
        public IReadOnlyDictionary<string, LocationDefinition> Locations => locations;
        public IReadOnlyDictionary<string, RouteDefinition> Routes => routes;
        public IReadOnlyDictionary<string, SearchPointDefinition> SearchPoints => searchPoints;

        public ItemDefinition GetItem(string id) => Get(items, id, "item");
        public LocationDefinition GetLocation(string id) => Get(locations, id, "location");
        public RouteDefinition GetRoute(string id) => Get(routes, id, "route");
        public SearchPointDefinition GetSearchPoint(string id) => Get(searchPoints, id, "search point");

        public bool TryGetItem(string id, out ItemDefinition definition) =>
            items.TryGetValue(id ?? string.Empty, out definition);

        public bool TryGetLocation(string id, out LocationDefinition definition) =>
            locations.TryGetValue(id ?? string.Empty, out definition);

        public bool TryGetRoute(string id, out RouteDefinition definition) =>
            routes.TryGetValue(id ?? string.Empty, out definition);

        public bool TryGetSearchPoint(string id, out SearchPointDefinition definition) =>
            searchPoints.TryGetValue(id ?? string.Empty, out definition);

        /// <summary>Trả về false nếu ID trùng — loader gom lỗi thay vì ném ngoại lệ.</summary>
        internal bool TryAdd<T>(T definition) where T : DefinitionBase
        {
            return definition switch
            {
                ItemDefinition d => items.TryAdd(d.Id, d),
                LocationDefinition d => locations.TryAdd(d.Id, d),
                RouteDefinition d => routes.TryAdd(d.Id, d),
                SearchPointDefinition d => searchPoints.TryAdd(d.Id, d),
                _ => false,
            };
        }

        static T Get<T>(Dictionary<string, T> source, string id, string label)
        {
            if (id != null && source.TryGetValue(id, out var definition)) return definition;
            throw new KeyNotFoundException($"Không tìm thấy {label} '{id}' trong DefinitionRegistry.");
        }
    }
}
