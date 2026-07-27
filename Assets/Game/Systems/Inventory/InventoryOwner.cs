using System.Collections.Generic;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Systems.Inventory
{
    public enum InventoryOwnerKind
    {
        Player,
        ShelterStorage,
        SearchPoint,
        DroppedItems,
    }

    /// <summary>
    /// Định danh một "túi đồ" bất kỳ trong world — không phải state, chỉ dùng làm tham số
    /// lệnh (Transfer) để một command chung phục vụ Search/Storage/Drop thay vì viết
    /// riêng từng cặp nguồn-đích.
    /// </summary>
    public readonly struct InventoryOwner
    {
        public readonly InventoryOwnerKind Kind;

        /// <summary>LocationId (ShelterStorage/DroppedItems) hoặc SearchPointId (SearchPoint). Null cho Player.</summary>
        public readonly string Id;

        InventoryOwner(InventoryOwnerKind kind, string id)
        {
            Kind = kind;
            Id = id;
        }

        public static readonly InventoryOwner Player = new(InventoryOwnerKind.Player, null);
        public static InventoryOwner ShelterStorage(string locationId) => new(InventoryOwnerKind.ShelterStorage, locationId);
        public static InventoryOwner SearchPoint(string searchPointId) => new(InventoryOwnerKind.SearchPoint, searchPointId);
        public static InventoryOwner DroppedItems(string locationId) => new(InventoryOwnerKind.DroppedItems, locationId);

        public override string ToString() => Id != null ? $"{Kind}:{Id}" : Kind.ToString();
    }

    /// <summary>
    /// Quy đổi <see cref="InventoryOwner"/> ra danh sách đồ thật, có tính riêng luật Carried
    /// Object (vật hai tay không nằm trong Slots như đồ thường).
    /// </summary>
    public static class InventoryOwnerOps
    {
        /// <summary>Null nghĩa là owner không hợp lệ lúc này (vd search point chưa mở).</summary>
        public static List<ItemInstanceState> ResolveSlots(
            WorldState world, DefinitionRegistry definitions, InventoryOwner owner)
        {
            switch (owner.Kind)
            {
                case InventoryOwnerKind.Player:
                    return world.Player.Inventory.Slots;

                case InventoryOwnerKind.ShelterStorage:
                    return world.GetOrCreateLocation(owner.Id).StorageContainer;

                case InventoryOwnerKind.DroppedItems:
                    return world.GetOrCreateLocation(owner.Id).DroppedItems;

                case InventoryOwnerKind.SearchPoint:
                    if (!definitions.TryGetSearchPoint(owner.Id, out var definition)) return null;
                    var location = world.GetOrCreateLocation(definition.LocationId);
                    return location.SearchPoints.TryGetValue(owner.Id, out var state) && state.Rolled
                        ? state.RemainingItems
                        : null;

                default:
                    return null;
            }
        }

        public static int CountOf(
            WorldState world, DefinitionRegistry definitions, InventoryOwner owner, string itemId)
        {
            if (owner.Kind == InventoryOwnerKind.Player
                && definitions.TryGetItem(itemId, out var item) && item.TwoHandCarry)
            {
                return world.Player.Inventory.CarriedObjectItemId == itemId ? 1 : 0;
            }

            var slots = ResolveSlots(world, definitions, owner);
            return slots != null ? InventoryOps.CountOf(slots, itemId) : 0;
        }

        public static bool CanAdd(
            WorldState world, DefinitionRegistry definitions, InventoryOwner owner,
            string itemId, int quantity)
        {
            if (owner.Kind != InventoryOwnerKind.Player) return ResolveSlots(world, definitions, owner) != null;

            return InventorySystem.CanAdd(
                world.Player.Inventory, definitions, definitions.Balance.Inventory, itemId, quantity);
        }

        /// <summary>Chuyển đồ giữa hai owner. Caller phải validate trước — không tự kiểm tra lại.</summary>
        public static void Move(
            WorldState world, DefinitionRegistry definitions,
            InventoryOwner from, InventoryOwner to, string itemId, int quantity)
        {
            bool isCarried = definitions.TryGetItem(itemId, out var item) && item.TwoHandCarry;

            if (from.Kind == InventoryOwnerKind.Player && isCarried)
            {
                world.Player.Inventory.CarriedObjectItemId = null;
            }
            else
            {
                var fromSlots = ResolveSlots(world, definitions, from);
                InventoryOps.RemoveItem(fromSlots, itemId, quantity);
            }

            if (to.Kind == InventoryOwnerKind.Player && isCarried)
            {
                world.Player.Inventory.CarriedObjectItemId = itemId;
            }
            else
            {
                var toSlots = ResolveSlots(world, definitions, to);
                InventoryOps.AddItem(toSlots, definitions, itemId, quantity);
            }
        }
    }
}
