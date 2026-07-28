using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Systems.Shelter
{
    public enum BuildRejectReason
    {
        None,
        UnknownSlot,
        UnknownModule,
        WrongZone,
        SlotOccupied,
        ConstructionInProgress,
        NotEnoughMaterials,
    }

    /// <summary>
    /// Xây Module vào Build Slot (BL-P3-03/04). Chỉ một construction chạy cùng lúc (MVP) —
    /// tick qua <see cref="ApplyShortTick"/> mỗi phút game nên chạy dù người chơi rời Shelter
    /// hay đang ngủ, không cần cơ chế Active/Passive Task riêng.
    /// </summary>
    public static class BuildSystem
    {
        public static BuildRejectReason CanStartConstruction(
            WorldState world, DefinitionRegistry definitions, string slotId, string moduleId)
        {
            var shelter = world.Shelter;

            if (!definitions.TryGetZoneForSlot(slotId, out var zone)) return BuildRejectReason.UnknownSlot;
            if (!definitions.TryGetModule(moduleId, out var module)) return BuildRejectReason.UnknownModule;
            if (!module.AllowedZoneIds.Contains(zone.Id)) return BuildRejectReason.WrongZone;
            if (shelter.BuildSlots.ContainsKey(slotId)) return BuildRejectReason.SlotOccupied;
            if (shelter.Construction != null) return BuildRejectReason.ConstructionInProgress;

            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            foreach (var pair in module.Materials)
            {
                if (InventoryOps.CountOf(storage, pair.Key) < pair.Value) return BuildRejectReason.NotEnoughMaterials;
            }

            return BuildRejectReason.None;
        }

        /// <summary>Caller phải gọi <see cref="CanStartConstruction"/> == None trước — không tự validate lại.</summary>
        public static void StartConstruction(WorldState world, DefinitionRegistry definitions, string slotId, string moduleId)
        {
            var module = definitions.GetModule(moduleId);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            foreach (var pair in module.Materials) InventoryOps.RemoveItem(storage, pair.Key, pair.Value);

            world.Shelter.Construction = new ConstructionState
            {
                SlotId = slotId,
                ModuleId = moduleId,
                MinutesRemaining = module.BuildMinutes,
            };
        }

        /// <summary>Huỷ công trình đang xây — không hoàn vật liệu (chưa có số liệu refund).</summary>
        public static bool CancelConstruction(WorldState world, string slotId)
        {
            var c = world.Shelter.Construction;
            if (c == null || c.SlotId != slotId) return false;
            world.Shelter.Construction = null;
            return true;
        }

        public static bool SetPaused(WorldState world, string slotId, bool paused)
        {
            var c = world.Shelter.Construction;
            if (c == null || c.SlotId != slotId) return false;
            c.Paused = paused;
            return true;
        }

        /// <summary>Tháo Module đã xây — không hoàn vật liệu (dismantle cơ bản, BL-P3-03).</summary>
        public static bool DismantleModule(WorldState world, string slotId) =>
            world.Shelter.BuildSlots.Remove(slotId);

        /// <summary>Trả về (SlotId, ModuleId) vừa hoàn thành trong tick này, null nếu chưa xong.</summary>
        public static (string SlotId, string ModuleId)? ApplyShortTick(WorldState world)
        {
            var shelter = world.Shelter;
            var c = shelter.Construction;
            if (c == null || c.Paused) return null;

            c.MinutesRemaining -= 1f;
            if (c.MinutesRemaining > 0f) return null;

            shelter.BuildSlots[c.SlotId] = new BuiltModuleState { ModuleId = c.ModuleId };
            shelter.Construction = null;
            return (c.SlotId, c.ModuleId);
        }
    }
}
