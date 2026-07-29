using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Shelter
{
    public enum BuildRejectReason
    {
        None,
        UnknownZone,
        UnknownModule,
        WrongZone,
        OutOfBounds,
        Overlapping,
        ConstructionInProgress,
        NotEnoughMaterials,
        NotEnoughPackedModules,
    }

    /// <summary>
    /// Xây Module — đặt tự do (world position) trong Zone hợp lệ (Free Placement, BL-P3-03,
    /// xem docs/plans/2026-07-28-free-placement.md). Chỉ một construction chạy cùng lúc (MVP)
    /// — tick qua <see cref="ApplyShortTick"/> mỗi phút game nên chạy dù người chơi rời Shelter
    /// hay đang ngủ, không cần cơ chế Active/Passive Task riêng. Tháo Module tạo ra 1 item
    /// "đã gói" (<see cref="ModuleDefinition.PackedItemId"/>) đặt lại được tức thì ở chỗ khác
    /// qua <see cref="RedeployModule"/> — không tốn Materials/BuildMinutes như xây mới (user
    /// yêu cầu 2026-07-29, xem docs/plans tương ứng).
    /// </summary>
    public static class BuildSystem
    {
        /// <summary>Kiểm tra hình học thuần (Zone/Bounds/Overlap) — dùng chung cho cả xây mới
        /// (<see cref="CanPlaceAt"/>) lẫn đặt lại Module đã gói (<see cref="CanRedeployAt"/>).
        /// Không gồm check tài nguyên (Materials/PackedItemId) hay ConstructionInProgress —
        /// hai luồng đòi tài nguyên khác nhau, ConstructionInProgress chỉ áp dụng xây mới vì
        /// Redeploy không đụng ConstructionState.</summary>
        static BuildRejectReason CanPlaceGeometry(
            WorldState world, DefinitionRegistry definitions, string zoneId, float x, float y, string moduleId,
            out ModuleDefinition module)
        {
            module = null;
            if (!definitions.TryGetShelterZone(zoneId, out var zone)) return BuildRejectReason.UnknownZone;
            if (!definitions.TryGetModule(moduleId, out module)) return BuildRejectReason.UnknownModule;
            if (!module.AllowedZoneIds.Contains(zoneId)) return BuildRejectReason.WrongZone;
            if (!zone.Contains(x, y)) return BuildRejectReason.OutOfBounds;

            foreach (var placed in world.Shelter.PlacedModules.Values)
            {
                if (!definitions.TryGetModule(placed.ModuleId, out var placedModule)) continue;
                float minDistance = module.FootprintRadius + placedModule.FootprintRadius;
                float dx = placed.PositionX - x, dy = placed.PositionY - y;
                if (dx * dx + dy * dy < minDistance * minDistance) return BuildRejectReason.Overlapping;
            }

            return BuildRejectReason.None;
        }

        public static BuildRejectReason CanPlaceAt(
            WorldState world, DefinitionRegistry definitions, string zoneId, float x, float y, string moduleId)
        {
            if (world.Shelter.Construction != null) return BuildRejectReason.ConstructionInProgress;

            var reason = CanPlaceGeometry(world, definitions, zoneId, x, y, moduleId, out var module);
            if (reason != BuildRejectReason.None) return reason;

            if (!HasEnoughMaterials(world, module)) return BuildRejectReason.NotEnoughMaterials;

            return BuildRejectReason.None;
        }

        /// <summary>Đặt lại Module đã Tháo (đã gói, xem <see cref="RedeployModule"/>) — không
        /// chặn bởi ConstructionInProgress vì không đụng ConstructionState.</summary>
        public static BuildRejectReason CanRedeployAt(
            WorldState world, DefinitionRegistry definitions, string zoneId, float x, float y, string moduleId)
        {
            var reason = CanPlaceGeometry(world, definitions, zoneId, x, y, moduleId, out var module);
            if (reason != BuildRejectReason.None) return reason;

            if (string.IsNullOrEmpty(module.PackedItemId)) return BuildRejectReason.UnknownModule;

            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            if (InventoryOps.CountOf(storage, module.PackedItemId) < 1) return BuildRejectReason.NotEnoughPackedModules;

            return BuildRejectReason.None;
        }

        /// <summary>Không phụ thuộc vị trí đặt — dùng để chặn sớm ở UI (nút "Chọn vị trí")
        /// trước khi vào Placement Mode, thay vì để người chơi rê ghost rồi mới biết.</summary>
        public static bool HasEnoughMaterials(WorldState world, ModuleDefinition module)
        {
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            foreach (var pair in module.Materials)
            {
                if (InventoryOps.CountOf(storage, pair.Key) < pair.Value) return false;
            }
            return true;
        }

        /// <summary>Caller phải gọi <see cref="CanPlaceAt"/> == None trước — không tự validate lại.</summary>
        public static void StartConstruction(
            WorldState world, DefinitionRegistry definitions, string zoneId, float x, float y, string moduleId)
        {
            var module = definitions.GetModule(moduleId);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            foreach (var pair in module.Materials) InventoryOps.RemoveItem(storage, pair.Key, pair.Value);

            world.Shelter.Construction = new ConstructionState
            {
                ZoneId = zoneId,
                ModuleId = moduleId,
                PositionX = x,
                PositionY = y,
                MinutesRemaining = module.BuildMinutes,
            };
        }

        /// <summary>Huỷ công trình đang xây — không hoàn vật liệu (chưa có số liệu refund).</summary>
        public static bool CancelConstruction(WorldState world)
        {
            if (world.Shelter.Construction == null) return false;
            world.Shelter.Construction = null;
            return true;
        }

        public static bool SetPaused(WorldState world, bool paused)
        {
            var c = world.Shelter.Construction;
            if (c == null) return false;
            c.Paused = paused;
            return true;
        }

        /// <summary>Caller phải gọi <see cref="CanRedeployAt"/> == None trước — không tự validate
        /// lại. Trừ 1 PackedItemId khỏi Storage, tạo <see cref="BuiltModuleState"/> ngay lập tức
        /// (không qua ConstructionState/BuildMinutes — đặt lại Module đã tháo, không phải xây
        /// mới). Trả về placementId mới.</summary>
        public static string RedeployModule(
            WorldState world, DefinitionRegistry definitions, string zoneId, float x, float y, string moduleId)
        {
            var module = definitions.GetModule(moduleId);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            InventoryOps.RemoveItem(storage, module.PackedItemId, 1);

            var shelter = world.Shelter;
            string placementId = $"placed_{++shelter.NextPlacementId}";
            shelter.PlacedModules[placementId] = new BuiltModuleState
            {
                ModuleId = moduleId,
                ZoneId = zoneId,
                PositionX = x,
                PositionY = y,
            };
            return placementId;
        }

        /// <summary>Tháo Module đã xây — thành 1 item "đã gói" (<see cref="ModuleDefinition.PackedItemId"/>)
        /// vào Storage, đặt lại tức thì qua <see cref="RedeployModule"/> (không hoàn Materials
        /// rời rạc — Module coi như được bê nguyên đi chỗ khác, không tháo rã thành phụ tùng).</summary>
        public static bool DismantleModule(WorldState world, DefinitionRegistry definitions, string placementId)
        {
            if (!world.Shelter.PlacedModules.TryGetValue(placementId, out var built)) return false;
            world.Shelter.PlacedModules.Remove(placementId);

            if (definitions.TryGetModule(built.ModuleId, out var module) && !string.IsNullOrEmpty(module.PackedItemId))
            {
                var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
                InventoryOps.AddItem(storage, definitions, module.PackedItemId, 1);
            }

            return true;
        }

        /// <summary>Trả về (PlacementId, ModuleId) vừa hoàn thành trong tick này, null nếu chưa xong.</summary>
        public static (string PlacementId, string ModuleId)? ApplyShortTick(WorldState world)
        {
            var shelter = world.Shelter;
            var c = shelter.Construction;
            if (c == null || c.Paused) return null;

            c.MinutesRemaining -= 1f;
            if (c.MinutesRemaining > 0f) return null;

            string placementId = $"placed_{++shelter.NextPlacementId}";
            shelter.PlacedModules[placementId] = new BuiltModuleState
            {
                ModuleId = c.ModuleId,
                ZoneId = c.ZoneId,
                PositionX = c.PositionX,
                PositionY = c.PositionY,
            };
            shelter.Construction = null;
            return (placementId, c.ModuleId);
        }
    }
}
