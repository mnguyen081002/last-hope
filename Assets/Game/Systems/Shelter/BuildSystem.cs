using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Inventory;

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
        RotationNotAllowed,
        NothingToClaim,
        InventoryFull,
    }

    /// <summary>
    /// Production (chế tạo Module, không gắn vị trí) tách khỏi Placement (đặt Module đã gói vào
    /// world position tự do trong Zone hợp lệ — Free Placement, BL-P3-03) — đổi 2026-07-30, xem
    /// docs/plans/2026-07-30-module-production-placement-loop.md. Chỉ một Production chạy cùng
    /// lúc (MVP) — tick qua <see cref="ApplyShortTick"/> mỗi phút game nên chạy dù người chơi rời
    /// Shelter hay đang ngủ, không cần cơ chế Active/Passive Task riêng. Production xong chuyển
    /// "Ready to Claim" — Nhận (<see cref="ClaimProduction"/>) mới thật sự cộng packed item
    /// (<see cref="ModuleDefinition.PackedItemId"/>) vào túi Player. Tháo Module cũng tạo ra
    /// packed item vào cùng túi Player — cả hai nguồn đặt lại tức thì qua
    /// <see cref="RedeployModule"/>, không tốn Materials/BuildMinutes.
    /// </summary>
    public static class BuildSystem
    {
        /// <summary>Kiểm tra hình học thuần (Zone/Bounds/Overlap) — dùng cho
        /// <see cref="CanRedeployAt"/> (đặt Module đã gói, nguồn duy nhất đi qua Placement Mode).
        /// Không gồm check tài nguyên (PackedItemId) hay ConstructionInProgress — Redeploy không
        /// đụng ConstructionState.</summary>
        static BuildRejectReason CanPlaceGeometry(
            WorldState world, DefinitionRegistry definitions, string zoneId, float x, float y, string moduleId,
            int rotationQuarterTurns, out ModuleDefinition module)
        {
            module = null;
            if (!definitions.TryGetShelterZone(zoneId, out var zone)) return BuildRejectReason.UnknownZone;
            if (!definitions.TryGetModule(moduleId, out module)) return BuildRejectReason.UnknownModule;
            if (!module.AllowedZoneIds.Contains(zoneId)) return BuildRejectReason.WrongZone;
            int rotation = NormalizeQuarterTurns(rotationQuarterTurns);
            if (rotation != 0 && !module.IsRotatable) return BuildRejectReason.RotationNotAllowed;

            GetFootprint(module, rotation, out float width, out float height);
            if (x - width / 2f < zone.BoundsMinX || x + width / 2f > zone.BoundsMaxX
                || y - height / 2f < zone.BoundsMinY || y + height / 2f > zone.BoundsMaxY)
                return BuildRejectReason.OutOfBounds;

            foreach (var placed in world.Shelter.PlacedModules.Values)
            {
                if (!definitions.TryGetModule(placed.ModuleId, out var placedModule)) continue;
                if (definitions.TryGetShelterZone(placed.ZoneId, out var placedZone)
                    && placedZone.Floor != zone.Floor)
                    continue;

                GetFootprint(placedModule, placed.RotationQuarterTurns,
                    out float placedWidth, out float placedHeight);
                float dx = UnityEngine.Mathf.Abs(placed.PositionX - x);
                float dy = UnityEngine.Mathf.Abs(placed.PositionY - y);
                if (dx < (width + placedWidth) / 2f && dy < (height + placedHeight) / 2f)
                    return BuildRejectReason.Overlapping;
            }

            return BuildRejectReason.None;
        }

        /// <summary>Đặt Module đã gói (Claim hoặc Tháo, xem <see cref="RedeployModule"/>) — không
        /// chặn bởi ConstructionInProgress vì không đụng ConstructionState. Packed item nằm
        /// trong túi Player (đổi 2026-07-30 — trước đó nằm ở Storage).</summary>
        public static BuildRejectReason CanRedeployAt(
            WorldState world, DefinitionRegistry definitions, string zoneId, float x, float y, string moduleId,
            int rotationQuarterTurns = 0)
        {
            var reason = CanPlaceGeometry(
                world, definitions, zoneId, x, y, moduleId, rotationQuarterTurns, out var module);
            if (reason != BuildRejectReason.None) return reason;

            if (string.IsNullOrEmpty(module.PackedItemId)) return BuildRejectReason.UnknownModule;

            // Packed item (item_packed_*) là TwoHandCarry — nằm ở CarriedObjectItemId, không phải
            // Slots. InventoryOwnerOps.CountOf đã biết phân biệt hai đường này (khác InventoryOps
            // thuần chỉ đọc Slots).
            if (InventoryOwnerOps.CountOf(world, definitions, InventoryOwner.Player, module.PackedItemId) < 1)
                return BuildRejectReason.NotEnoughPackedModules;

            return BuildRejectReason.None;
        }

        /// <summary>Không phụ thuộc vị trí đặt — dùng để chặn sớm ở UI (nút "Sản xuất")
        /// trước khi trừ Materials, thay vì để trừ rồi mới biết thiếu.</summary>
        public static bool HasEnoughMaterials(WorldState world, ModuleDefinition module)
        {
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            foreach (var pair in module.Materials)
            {
                if (InventoryOps.CountOf(storage, pair.Key) < pair.Value) return false;
            }
            return true;
        }

        public static int NormalizeQuarterTurns(int quarterTurns) =>
            ((quarterTurns % 4) + 4) % 4;

        public static void GetFootprint(
            ModuleDefinition module, int rotationQuarterTurns, out float width, out float height)
        {
            width = module.FootprintWidth > 0f ? module.FootprintWidth : module.FootprintRadius * 2f;
            height = module.FootprintHeight > 0f ? module.FootprintHeight : module.FootprintRadius * 2f;
            if ((NormalizeQuarterTurns(rotationQuarterTurns) & 1) == 1)
                (width, height) = (height, width);
        }

        /// <summary>Không gắn vị trí/Zone — Production tách khỏi Placement (đổi 2026-07-30). Chỉ
        /// chặn bởi Module không tồn tại, đang có Production khác chạy, hoặc thiếu Materials.</summary>
        public static BuildRejectReason CanStartProduction(
            WorldState world, DefinitionRegistry definitions, string moduleId, out ModuleDefinition module)
        {
            if (!definitions.TryGetModule(moduleId, out module)) return BuildRejectReason.UnknownModule;
            if (world.Shelter.Construction != null) return BuildRejectReason.ConstructionInProgress;
            if (!HasEnoughMaterials(world, module)) return BuildRejectReason.NotEnoughMaterials;

            return BuildRejectReason.None;
        }

        /// <summary>Caller phải gọi <see cref="CanStartProduction"/> == None trước — không tự
        /// validate lại.</summary>
        public static void StartConstruction(WorldState world, DefinitionRegistry definitions, string moduleId)
        {
            var module = definitions.GetModule(moduleId);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            foreach (var pair in module.Materials) InventoryOps.RemoveItem(storage, pair.Key, pair.Value);

            world.Shelter.Construction = new ConstructionState
            {
                ModuleId = moduleId,
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
        /// lại. Trừ 1 PackedItemId khỏi túi Player, tạo <see cref="BuiltModuleState"/> ngay lập
        /// tức (không qua ConstructionState/BuildMinutes — đặt Module đã gói sẵn, không phải
        /// Production). Trả về placementId mới.</summary>
        public static string RedeployModule(
            WorldState world, DefinitionRegistry definitions, string zoneId, float x, float y, string moduleId,
            int rotationQuarterTurns = 0)
        {
            var module = definitions.GetModule(moduleId);
            RemovePackedItemFromPlayer(world, definitions, module.PackedItemId);

            var shelter = world.Shelter;
            string placementId = $"placed_{++shelter.NextPlacementId}";
            shelter.PlacedModules[placementId] = new BuiltModuleState
            {
                ModuleId = moduleId,
                ZoneId = zoneId,
                PositionX = x,
                PositionY = y,
                RotationQuarterTurns = NormalizeQuarterTurns(rotationQuarterTurns),
            };
            return placementId;
        }

        /// <summary>Packed item là TwoHandCarry — nằm ở <see cref="InventoryState.CarriedObjectItemId"/>,
        /// không phải Slots (khác đồ thường). Dùng bởi <see cref="RedeployModule"/> sau khi
        /// <see cref="CanRedeployAt"/> đã xác nhận có đủ.</summary>
        static void RemovePackedItemFromPlayer(WorldState world, DefinitionRegistry definitions, string itemId)
        {
            if (definitions.TryGetItem(itemId, out var item) && item.TwoHandCarry)
                world.Player.Inventory.CarriedObjectItemId = null;
            else
                InventoryOps.RemoveItem(world.Player.Inventory, itemId, 1);
        }

        /// <summary>Không mutate — dùng để chặn Tháo nếu túi Player không còn chỗ nhận packed
        /// item (edge case "Remove": không tháo được nếu không trả về được).</summary>
        public static BuildRejectReason CanDismantle(WorldState world, DefinitionRegistry definitions, string placementId)
        {
            if (!world.Shelter.PlacedModules.TryGetValue(placementId, out var built))
                return BuildRejectReason.UnknownModule;
            if (!definitions.TryGetModule(built.ModuleId, out var module) || string.IsNullOrEmpty(module.PackedItemId))
                return BuildRejectReason.None;

            if (!InventorySystem.CanAdd(
                    world.Player.Inventory, definitions, definitions.Balance.Inventory, module.PackedItemId, 1))
                return BuildRejectReason.InventoryFull;

            return BuildRejectReason.None;
        }

        /// <summary>Tháo Module đã xây — thành 1 item "đã gói" (<see cref="ModuleDefinition.PackedItemId"/>)
        /// vào túi Player (đổi 2026-07-30 — trước đó vào Storage), đặt lại tức thì qua
        /// <see cref="RedeployModule"/> (không hoàn Materials rời rạc — Module coi như được bê
        /// nguyên đi chỗ khác, không tháo rã thành phụ tùng).</summary>
        public static bool DismantleModule(WorldState world, DefinitionRegistry definitions, string placementId)
        {
            if (!world.Shelter.PlacedModules.TryGetValue(placementId, out var built)) return false;
            world.Shelter.PlacedModules.Remove(placementId);

            if (definitions.TryGetModule(built.ModuleId, out var module) && !string.IsNullOrEmpty(module.PackedItemId))
            {
                InventorySystem.Add(world.Player.Inventory, definitions, module.PackedItemId, 1);
            }

            return true;
        }

        /// <summary>Trả về ModuleId vừa hoàn thành Production trong tick này (chuyển "Ready to
        /// Claim", không tạo <see cref="BuiltModuleState"/> — đổi 2026-07-30), null nếu chưa xong.</summary>
        public static string ApplyShortTick(WorldState world)
        {
            var shelter = world.Shelter;
            var c = shelter.Construction;
            if (c == null || c.Paused) return null;

            c.MinutesRemaining -= 1f;
            if (c.MinutesRemaining > 0f) return null;

            shelter.ReadyToClaim.TryGetValue(c.ModuleId, out int count);
            shelter.ReadyToClaim[c.ModuleId] = count + 1;

            string moduleId = c.ModuleId;
            shelter.Construction = null;
            return moduleId;
        }

        /// <summary>Không mutate — chặn Nhận nếu không có gì Ready hoặc túi Player không còn chỗ.</summary>
        public static BuildRejectReason CanClaim(
            WorldState world, DefinitionRegistry definitions, string moduleId, out ModuleDefinition module)
        {
            module = null;
            if (!definitions.TryGetModule(moduleId, out module)) return BuildRejectReason.UnknownModule;
            if (!world.Shelter.ReadyToClaim.TryGetValue(moduleId, out int count) || count <= 0)
                return BuildRejectReason.NothingToClaim;

            if (!InventorySystem.CanAdd(
                    world.Player.Inventory, definitions, definitions.Balance.Inventory, module.PackedItemId, 1))
                return BuildRejectReason.InventoryFull;

            return BuildRejectReason.None;
        }

        /// <summary>Caller phải gọi <see cref="CanClaim"/> == None trước — không tự validate lại.</summary>
        public static void ClaimProduction(WorldState world, DefinitionRegistry definitions, string moduleId)
        {
            var module = definitions.GetModule(moduleId);
            var shelter = world.Shelter;

            int remaining = shelter.ReadyToClaim[moduleId] - 1;
            if (remaining <= 0) shelter.ReadyToClaim.Remove(moduleId);
            else shelter.ReadyToClaim[moduleId] = remaining;

            InventorySystem.Add(world.Player.Inventory, definitions, module.PackedItemId, 1);
        }

        /// <summary>Tìm Module ứng với 1 packed item — dùng ở InventoryPanel để hiện nút "Đặt"
        /// cạnh item trong túi. 5 Module trong content hiện tại nên quét thẳng, không cần index
        /// ngược trong DefinitionRegistry.</summary>
        public static bool TryFindModuleByPackedItem(
            DefinitionRegistry definitions, string itemId, out ModuleDefinition module)
        {
            foreach (var candidate in definitions.Modules.Values)
            {
                if (candidate.PackedItemId == itemId)
                {
                    module = candidate;
                    return true;
                }
            }
            module = null;
            return false;
        }
    }
}
