using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using Newtonsoft.Json;

namespace LastHope.Systems.Telemetry
{
    /// <summary>
    /// JSONL telemetry (BL-P1-21). Appends one line per event with File.AppendAllText — no held
    /// file handle, so a crash mid-session doesn't corrupt or lose earlier lines.
    /// </summary>
    public sealed class TelemetryLogger
    {
        private readonly string _filePath;
        private readonly GameContext _ctx;
        private readonly string _sessionId;

        public TelemetryLogger(string directory, GameContext ctx, string sessionId)
        {
            _ctx = ctx;
            _sessionId = sessionId;
            Directory.CreateDirectory(directory);
            _filePath = Path.Combine(directory, $"session_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jsonl");

            ctx.Events.Subscribe<TravelStarted>(OnTravelStarted);
            ctx.Events.Subscribe<TravelCompleted>(OnTravelCompleted);
            ctx.Events.Subscribe<SearchPointOpened>(OnSearchPointOpened);
            ctx.Events.Subscribe<ItemTransferred>(OnItemTransferred);
        }

        private void OnTravelStarted(TravelStarted e) => Log("travel_started", new Dictionary<string, object>
        {
            ["route_id"] = e.RouteId,
            ["from"] = e.FromLocationId,
            ["to"] = e.ToLocationId,
            ["planned_minutes"] = e.PlannedMinutes,
        });

        private void OnTravelCompleted(TravelCompleted e)
        {
            var inv = _ctx.World.Player.Inventory;
            Log("travel_completed", new Dictionary<string, object>
            {
                ["route_id"] = e.RouteId,
                ["from"] = e.FromLocationId,
                ["to"] = e.ToLocationId,
                ["minutes_spent"] = e.MinutesSpent,
                ["carry_weight_kg"] = inv.CurrentWeightKg,
                ["carry_volume_l"] = inv.CurrentVolumeLiters,
                ["overload"] = inv.Overload.ToString(),
            });
        }

        private void OnSearchPointOpened(SearchPointOpened e) => Log("search_opened", new Dictionary<string, object>
        {
            ["search_point_id"] = e.SearchPointId,
            ["first_open"] = e.FirstOpen,
        });

        private void OnItemTransferred(ItemTransferred e)
        {
            if (e.DestinationOwnerId == _ctx.World.Player.ActorId)
            {
                Log("item_collected", new Dictionary<string, object>
                {
                    ["item_id"] = e.ItemId,
                    ["quantity"] = e.Quantity,
                    ["source"] = e.SourceOwnerId,
                });
            }
        }

        /// <summary>Public so UI-originated events (inventory_open_time, item_left_behind) can log too.</summary>
        public void Log(string eventName, IDictionary<string, object> payload)
        {
            var line = new Dictionary<string, object>
            {
                ["ts_utc"] = DateTime.UtcNow.ToString("O"),
                ["world_minute"] = _ctx.World.WorldTimeMinutes,
                ["session_id"] = _sessionId,
                ["playthrough_id"] = _ctx.World.PlaythroughId,
                ["event"] = eventName,
                ["payload"] = payload,
            };

            try
            {
                File.AppendAllText(_filePath, JsonConvert.SerializeObject(line) + "\n", Encoding.UTF8);
            }
            catch
            {
                // Telemetry must never crash gameplay.
            }
        }
    }
}
