using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LastHope.Data.Definitions;

namespace LastHope.Data
{
    public class DefinitionLoadException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public DefinitionLoadException(IReadOnlyList<string> errors)
            : base($"Definition không hợp lệ ({errors.Count} lỗi):\n- " + string.Join("\n- ", errors))
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// Đọc toàn bộ `.json` trong thư mục Definitions. Loại definition xác định theo tiền tố
    /// tên file, nên thêm content = thêm file, không sửa code.
    ///
    /// Nguyên tắc: **gom toàn bộ lỗi rồi mới ném** — fail-first sẽ giấu các lỗi content còn
    /// lại và bắt sửa từng vòng một.
    /// </summary>
    public static class DefinitionLoader
    {
        class Manifest
        {
            public string DefinitionVersion;
        }

        /// <summary>Tiền tố file chưa có kiểu tương ứng (milestone sau) — bỏ qua, không báo lỗi.</summary>
        static readonly string[] DeferredPrefixes =
        {
            "events_", "npcs_", "phases_",
        };

        public static DefinitionRegistry LoadFromDirectory(string directory)
        {
            var registry = new DefinitionRegistry();
            var errors = new List<string>();

            if (!Directory.Exists(directory))
            {
                throw new DefinitionLoadException(new[] { $"Không thấy thư mục '{directory}'." });
            }

            foreach (string path in Directory.GetFiles(directory, "*.json").OrderBy(p => p))
            {
                LoadFile(path, registry, errors);
            }

            Validate(registry, errors);

            if (errors.Count > 0) throw new DefinitionLoadException(errors);
            return registry;
        }

        static void LoadFile(string path, DefinitionRegistry registry, List<string> errors)
        {
            string file = Path.GetFileName(path);
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                errors.Add($"{file}: không đọc được — {e.Message}");
                return;
            }

            try
            {
                if (file == "manifest.json")
                {
                    registry.DefinitionVersion =
                        DefinitionJson.Deserialize<Manifest>(json)?.DefinitionVersion ?? "0.0.0";
                }
                else if (file == "balance.json")
                {
                    registry.Balance = DefinitionJson.Deserialize<BalanceDefinition>(json) ?? new BalanceDefinition();
                }
                else if (file.StartsWith("items_", StringComparison.Ordinal))
                {
                    AddAll(DefinitionJson.Deserialize<List<ItemDefinition>>(json), file, registry, errors);
                }
                else if (file.StartsWith("locations_", StringComparison.Ordinal))
                {
                    AddAll(DefinitionJson.Deserialize<List<LocationDefinition>>(json), file, registry, errors);
                }
                else if (file.StartsWith("routes_", StringComparison.Ordinal))
                {
                    AddAll(DefinitionJson.Deserialize<List<RouteDefinition>>(json), file, registry, errors);
                }
                else if (file.StartsWith("searchpoints_", StringComparison.Ordinal))
                {
                    AddAll(DefinitionJson.Deserialize<List<SearchPointDefinition>>(json), file, registry, errors);
                }
                else if (file.StartsWith("modules_", StringComparison.Ordinal))
                {
                    AddAll(DefinitionJson.Deserialize<List<ModuleDefinition>>(json), file, registry, errors);
                }
                else if (file.StartsWith("shelterzones_", StringComparison.Ordinal))
                {
                    AddAll(DefinitionJson.Deserialize<List<ShelterZoneDefinition>>(json), file, registry, errors);
                }
                else if (!DeferredPrefixes.Any(p => file.StartsWith(p, StringComparison.Ordinal)))
                {
                    errors.Add($"{file}: không nhận diện được loại definition từ tên file.");
                }
            }
            catch (Exception e)
            {
                errors.Add($"{file}: parse lỗi — {e.Message}");
            }
        }

        static void AddAll<T>(
            List<T> definitions, string file, DefinitionRegistry registry, List<string> errors)
            where T : DefinitionBase
        {
            if (definitions == null)
            {
                errors.Add($"{file}: nội dung rỗng hoặc không phải mảng.");
                return;
            }

            foreach (var definition in definitions)
            {
                if (string.IsNullOrWhiteSpace(definition?.Id))
                {
                    errors.Add($"{file}: có entry thiếu 'id'.");
                    continue;
                }

                if (!registry.TryAdd(definition))
                {
                    errors.Add($"{file}: ID trùng '{definition.Id}'.");
                }
            }
        }

        static void Validate(DefinitionRegistry registry, List<string> errors)
        {
            foreach (var location in registry.Locations.Values)
            {
                foreach (string routeId in location.ConnectedRouteIds)
                {
                    if (!registry.Routes.ContainsKey(routeId))
                        errors.Add($"location '{location.Id}': route không tồn tại '{routeId}'.");
                }

                foreach (string searchPointId in location.SearchPointIds)
                {
                    if (!registry.SearchPoints.ContainsKey(searchPointId))
                        errors.Add($"location '{location.Id}': search point không tồn tại '{searchPointId}'.");
                }
            }

            foreach (var route in registry.Routes.Values)
            {
                if (!registry.Locations.ContainsKey(route.FromLocationId))
                    errors.Add($"route '{route.Id}': from_location_id không tồn tại '{route.FromLocationId}'.");
                if (!registry.Locations.ContainsKey(route.ToLocationId))
                    errors.Add($"route '{route.Id}': to_location_id không tồn tại '{route.ToLocationId}'.");
            }

            foreach (var searchPoint in registry.SearchPoints.Values)
            {
                if (!registry.Locations.ContainsKey(searchPoint.LocationId))
                    errors.Add($"search point '{searchPoint.Id}': location không tồn tại '{searchPoint.LocationId}'.");

                foreach (var entry in searchPoint.LootTable)
                {
                    if (!registry.Items.ContainsKey(entry.ItemId))
                        errors.Add($"search point '{searchPoint.Id}': item không tồn tại '{entry.ItemId}'.");
                    if (entry.MinQuantity > entry.MaxQuantity)
                        errors.Add($"search point '{searchPoint.Id}': '{entry.ItemId}' có min > max quantity.");
                }
            }

            var balance = registry.Balance.NewGame;
            if (!string.IsNullOrEmpty(balance.StartLocationId)
                && !registry.Locations.ContainsKey(balance.StartLocationId))
            {
                errors.Add($"balance.new_game: start_location_id không tồn tại '{balance.StartLocationId}'.");
            }

            foreach (var module in registry.Modules.Values)
            {
                foreach (string zoneId in module.AllowedZoneIds)
                {
                    if (!registry.ShelterZones.ContainsKey(zoneId))
                        errors.Add($"module '{module.Id}': shelter zone không tồn tại '{zoneId}'.");
                }

                foreach (string itemId in module.Materials.Keys)
                {
                    if (!registry.Items.ContainsKey(itemId))
                        errors.Add($"module '{module.Id}': material item không tồn tại '{itemId}'.");
                }
            }
        }
    }
}
