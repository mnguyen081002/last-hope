using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Telemetry;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class TelemetryTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "LastHopeTelemetryTests_" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
        }

        [Test]
        public void TravelAndSearch_ProducesExpectedEventNamesInOrder()
        {
            var world = new WorldState { RandomSeed = 1 };
            world.Player.CurrentLocationId = "location_a";
            var bus = new EventBus();

            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_water"] = new ItemDefinition { Id = "item_water", BaseWeightKg = 0.8f, BaseVolumeLiters = 1f, MaxStackSize = 4 },
            };
            var locations = new Dictionary<string, LocationDefinition>
            {
                ["location_b"] = new LocationDefinition { Id = "location_b" },
            };
            var routes = new Dictionary<string, RouteDefinition>
            {
                ["route_a_b"] = new RouteDefinition { Id = "route_a_b", FromLocationId = "location_a", ToLocationId = "location_b", TravelMinutes = 10 },
            };
            var searchPoints = new Dictionary<string, SearchPointDefinition>
            {
                ["sp_1"] = new SearchPointDefinition
                {
                    Id = "sp_1",
                    LocationId = "location_b",
                    LootTable = new List<LootEntry> { new LootEntry { ItemId = "item_water", Weight = 1, MinQuantity = 1, MaxQuantity = 1 } },
                },
            };
            var registry = new DefinitionRegistry("test", new BalanceConfig(), items, locations, routes, searchPoints);
            var ctx = new GameContext(world, registry, bus, new RngService(world), new TickScheduler(world, bus));
            var processor = new CommandProcessor(ctx);

            _ = new TelemetryLogger(_tempDir, ctx, "session_test");

            processor.Submit(new BeginTravelCommand("player", "route_a_b"));
            processor.Submit(new OpenSearchPointCommand("player", "sp_1"));
            processor.Submit(new TransferItemCommand("searchpoint:sp_1", "sp_1_item_water_0", "player", 1));

            string[] files = Directory.GetFiles(_tempDir, "*.jsonl");
            Assert.AreEqual(1, files.Length);

            List<string> eventNames = File.ReadAllLines(files[0])
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => (string)JObject.Parse(line)["event"])
                .ToList();

            CollectionAssert.AreEqual(
                new[] { "travel_started", "travel_completed", "search_opened", "item_collected" }, eventNames);
        }
    }
}
