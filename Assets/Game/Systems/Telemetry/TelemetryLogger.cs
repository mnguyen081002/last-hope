using System;
using System.IO;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Systems.Registry;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LastHope.Systems.Telemetry
{
    /// <summary>
    /// Ghi JSONL vào <c>persistentDataPath/Telemetry</c>. Sự kiện có sẵn qua EventBus
    /// (travel, search opened) thì tự subscribe; sự kiện chỉ UI biết (đóng search panel,
    /// thời gian mở inventory) thì UI gọi thẳng <see cref="LogSearchClosed"/>/
    /// <see cref="LogInventoryOpenDuration"/>.
    /// </summary>
    public class TelemetryLogger
    {
        static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy(),
            },
        };

        readonly GameServices services;
        readonly string filePath;
        readonly string sessionId;

        public TelemetryLogger(GameServices services, string directory)
        {
            this.services = services;
            Directory.CreateDirectory(directory);

            sessionId = Guid.NewGuid().ToString("N");
            filePath = Path.Combine(directory, $"session_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jsonl");

            services.Events.Subscribe<TravelStarted>(OnTravelStarted);
            services.Events.Subscribe<LocationChanged>(OnLocationChanged);
            services.Events.Subscribe<SearchPointOpened>(OnSearchPointOpened);
        }

        void OnTravelStarted(TravelStarted e) =>
            Write("travel_started", new { route_id = e.RouteId, travel_minutes = e.TravelMinutes });

        void OnLocationChanged(LocationChanged e) =>
            Write("location_changed", new
            {
                from_location_id = e.FromLocationId,
                to_location_id = e.ToLocationId,
                carry_load_kg = InventoryOps.TotalWeightKg(services.World.Player.Inventory, services.Definitions),
            });

        void OnSearchPointOpened(SearchPointOpened e) =>
            Write("search_opened", new { search_point_id = e.SearchPointId });

        /// <summary>UI gọi khi đóng SearchPanel — chỉ UI biết số lượng lúc mở để so sánh.</summary>
        public void LogSearchClosed(string searchPointId, int itemsTaken, int itemsLeftBehind) =>
            Write("search_closed", new
            {
                search_point_id = searchPointId,
                items_taken = itemsTaken,
                items_left_behind = itemsLeftBehind,
            });

        /// <summary>UI gọi khi đóng InventoryPanel.</summary>
        public void LogInventoryOpenDuration(float durationSeconds) =>
            Write("inventory_open_time", new { duration_seconds = durationSeconds });

        void Write(string eventType, object data)
        {
            var line = new
            {
                session_id = sessionId,
                real_time_utc = DateTime.UtcNow.ToString("o"),
                world_time_minutes = services.World.WorldTimeMinutes,
                @event = eventType,
                data,
            };

            File.AppendAllText(filePath, JsonConvert.SerializeObject(line, Settings) + "\n");
        }
    }
}
