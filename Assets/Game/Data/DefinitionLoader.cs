using System;
using System.Collections.Generic;
using System.IO;
using LastHope.Data.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace LastHope.Data
{
    public sealed class DefinitionLoadResult
    {
        public bool Success { get; set; }
        public DefinitionRegistry Registry { get; set; }
        public List<string> Errors { get; } = new List<string>();
    }

    /// <summary>
    /// Loads all Definition JSON from a directory (technical-specification.md mục 9/§41).
    /// File-name prefix routes content to a type: manifest.json, items_*.json, locations_*.json,
    /// routes_*.json, searchpoints_*.json. Collects every validation error before returning
    /// (duplicate ids, dangling references, missing ids) — never fails on the first error.
    /// </summary>
    public static class DefinitionLoader
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Converters = { new StringEnumConverter() },
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public static DefinitionLoadResult Load(string directoryPath)
        {
            var result = new DefinitionLoadResult();

            if (!Directory.Exists(directoryPath))
            {
                result.Errors.Add($"Definitions directory not found: {directoryPath}");
                result.Success = false;
                return result;
            }

            string definitionVersion = LoadManifestVersion(directoryPath, result.Errors);
            BalanceConfig balance = LoadBalance(directoryPath);

            var items = LoadTyped<ItemDefinition>(directoryPath, "items_", result.Errors);
            var locations = LoadTyped<LocationDefinition>(directoryPath, "locations_", result.Errors);
            var routes = LoadTyped<RouteDefinition>(directoryPath, "routes_", result.Errors);
            var searchPoints = LoadTyped<SearchPointDefinition>(directoryPath, "searchpoints_", result.Errors);
            var disasterPhases = LoadTyped<DisasterPhaseDefinition>(directoryPath, "phases_", result.Errors);
            var shelterZones = LoadTyped<ShelterZoneDefinition>(directoryPath, "shelterzones_", result.Errors);

            Validate(items, locations, routes, searchPoints, result.Errors);
            ValidateDisasterPhases(disasterPhases, result.Errors);

            result.Registry = new DefinitionRegistry(definitionVersion, balance, items, locations, routes, searchPoints, disasterPhases, shelterZones);
            result.Success = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// balance.json is config, not a Definition list — missing/unparsable falls back to
        /// BalanceConfig defaults rather than failing the whole load (BL-P1-01 decision).
        /// </summary>
        private static BalanceConfig LoadBalance(string directoryPath)
        {
            string path = Path.Combine(directoryPath, "balance.json");
            if (!File.Exists(path)) return new BalanceConfig();

            try
            {
                return JsonConvert.DeserializeObject<BalanceConfig>(File.ReadAllText(path), JsonSettings)
                       ?? new BalanceConfig();
            }
            catch (Exception)
            {
                return new BalanceConfig();
            }
        }

        private static string LoadManifestVersion(string directoryPath, List<string> errors)
        {
            string manifestPath = Path.Combine(directoryPath, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                errors.Add("manifest.json not found; definition_version is unknown.");
                return "unknown";
            }

            try
            {
                var manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath), JsonSettings);
                if (string.IsNullOrEmpty(manifest?.DefinitionVersion))
                {
                    errors.Add("manifest.json is missing definition_version.");
                    return "unknown";
                }
                return manifest.DefinitionVersion;
            }
            catch (Exception e)
            {
                errors.Add($"manifest.json failed to parse: {e.Message}");
                return "unknown";
            }
        }

        private static Dictionary<string, T> LoadTyped<T>(string directoryPath, string prefix, List<string> errors)
            where T : DefinitionBase
        {
            var map = new Dictionary<string, T>();

            foreach (string filePath in Directory.GetFiles(directoryPath, prefix + "*.json"))
            {
                string fileName = Path.GetFileName(filePath);
                List<T> entries;
                try
                {
                    entries = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filePath), JsonSettings);
                }
                catch (Exception e)
                {
                    errors.Add($"{fileName} failed to parse: {e.Message}");
                    continue;
                }

                if (entries == null) continue;

                foreach (T entry in entries)
                {
                    if (string.IsNullOrEmpty(entry.Id))
                    {
                        errors.Add($"{fileName} contains an entry with a missing/empty id.");
                        continue;
                    }

                    if (!map.TryAdd(entry.Id, entry))
                        errors.Add($"Duplicate id '{entry.Id}' in {fileName} (already defined in this type).");
                }
            }

            return map;
        }

        private static void Validate(
            Dictionary<string, ItemDefinition> items,
            Dictionary<string, LocationDefinition> locations,
            Dictionary<string, RouteDefinition> routes,
            Dictionary<string, SearchPointDefinition> searchPoints,
            List<string> errors)
        {
            foreach (var location in locations.Values)
            {
                foreach (string spId in location.SearchPointIds)
                    if (!searchPoints.ContainsKey(spId))
                        errors.Add($"Location '{location.Id}' references missing search point '{spId}'.");

                foreach (string routeId in location.ConnectedRouteIds)
                    if (!routes.ContainsKey(routeId))
                        errors.Add($"Location '{location.Id}' references missing route '{routeId}'.");
            }

            foreach (var route in routes.Values)
            {
                if (!locations.ContainsKey(route.FromLocationId))
                    errors.Add($"Route '{route.Id}' references missing from_location_id '{route.FromLocationId}'.");
                if (!locations.ContainsKey(route.ToLocationId))
                    errors.Add($"Route '{route.Id}' references missing to_location_id '{route.ToLocationId}'.");
            }

            foreach (var searchPoint in searchPoints.Values)
            {
                if (!locations.ContainsKey(searchPoint.LocationId))
                    errors.Add($"SearchPoint '{searchPoint.Id}' references missing location '{searchPoint.LocationId}'.");

                foreach (var loot in searchPoint.LootTable)
                {
                    if (!items.ContainsKey(loot.ItemId))
                        errors.Add($"SearchPoint '{searchPoint.Id}' loot table references missing item '{loot.ItemId}'.");

                    if (loot.Chance < 0 || loot.Chance > 100)
                        errors.Add($"SearchPoint '{searchPoint.Id}' loot entry '{loot.ItemId}' has chance {loot.Chance}, must be 0-100.");
                }
            }
        }

        /// <summary>Empty is valid (no phases_*.json shipped yet, e.g. P1 fixtures). Once any
        /// phase exists, exactly one must start at minute 0 and StartMinute values must be unique
        /// — DisasterPhaseSystem walks them in StartMinute order to find "current phase".</summary>
        private static void ValidateDisasterPhases(Dictionary<string, DisasterPhaseDefinition> phases, List<string> errors)
        {
            if (phases.Count == 0) return;

            var seenStartMinutes = new HashSet<long>();
            bool hasZero = false;
            foreach (var phase in phases.Values)
            {
                if (!seenStartMinutes.Add(phase.StartMinute))
                    errors.Add($"Duplicate start_minute {phase.StartMinute} among disaster phases (phase '{phase.Id}').");
                if (phase.StartMinute == 0) hasZero = true;
            }
            if (!hasZero)
                errors.Add("No disaster phase defined at start_minute 0.");
        }

        private sealed class Manifest
        {
            public string DefinitionVersion { get; set; }
        }
    }
}
