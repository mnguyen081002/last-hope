using LastHope.Core.State;
using LastHope.Data;

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
    }

    /// <summary>
    /// Xây Module — đặt tự do (world position) trong Zone hợp lệ (Free Placement, BL-P3-03,
    /// xem docs/plans/2026-07-28-free-placement.md). Chỉ một construction chạy cùng lúc (MVP)
    /// — tick qua <see cref="ApplyShortTick"/> mỗi phút game nên chạy dù người chơi rời Shelter
    /// hay đang ngủ, không cần cơ chế Active/Passive Task riêng.
    /// </summary>
    public static class BuildSystem
    {
        public static BuildRejectReason CanPlaceAt(
            WorldState world, DefinitionRegistry definitions, string zoneId, float x, float y, string moduleId)
        {
            var shelter = world.Shelter;

            if (!definitions.TryGetShelterZone(zoneId, out var zone)) return BuildRejectReason.UnknownZone;
            if (!definitions.TryGetModule(moduleId, out var module)) return BuildRejectReason.UnknownModule;
            if (!module.AllowedZoneIds.Contains(zoneId)) return BuildRejectReason.WrongZone;
            if (!zone.Contains(x, y)) return BuildRejectReason.OutOfBounds;
            if (shelter.Construction != null) return BuildRejectReason.ConstructionInProgress;

            foreach (var placed in shelter.PlacedModules.Values)
            {
                if (!definitions.TryGetModule(placed.ModuleId, out var placedModule)) continue;
                float minDistance = module.FootprintRadius + placedModule.FootprintRadius;
                float dx = placed.PositionX - x, dy = placed.PositionY - y;
                if (dx * dx + dy * dy < minDistance * minDistance) return BuildRejectReason.Overlapping;
            }

            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            foreach (var pair in module.Materials)
            {
                if (InventoryOps.CountOf(storage, pair.Key) < pair.Value) return BuildRejectReason.NotEnoughMaterials;
            }

            return BuildRejectReason.None;
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

        /// <summary>Tháo Module đã xây — không hoàn vật liệu (dismantle cơ bản, BL-P3-03).</summary>
        public static bool DismantleModule(WorldState world, string placementId) =>
            world.Shelter.PlacedModules.Remove(placementId);

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
