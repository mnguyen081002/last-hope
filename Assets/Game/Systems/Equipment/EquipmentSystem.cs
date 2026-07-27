using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Inventory;

namespace LastHope.Systems.Equipment
{
    /// <summary>
    /// Mặc/tháo equipment và tính protection từ item đang mặc. Đồ mặc không nằm trong
    /// <see cref="InventoryState.Slots"/> (không tính vào Carry Load) — khớp
    /// <see cref="ItemDefinition.IsEquipment"/>.
    /// </summary>
    public static class EquipmentSystem
    {
        /// <summary>
        /// Mặc item từ túi vào đúng slot. Nếu slot đang có đồ, tự tháo cái cũ về túi trước.
        /// Trả về false nếu item không phải equipment, không có trong túi, hoặc tháo đồ cũ
        /// ra không đủ chỗ chứa (backpack đầy).
        /// </summary>
        public static bool TryEquip(PlayerState player, DefinitionRegistry definitions, string itemId)
        {
            if (!definitions.TryGetItem(itemId, out var item) || !item.IsEquipment) return false;
            if (InventoryOps.CountOf(player.Inventory, itemId) <= 0) return false;

            if (player.Equipped.TryGetValue(item.EquipSlot, out string currentItemId))
            {
                if (!TryUnequip(player, definitions, item.EquipSlot)) return false;
            }

            InventoryOps.RemoveItem(player.Inventory, itemId, 1);
            player.Equipped[item.EquipSlot] = itemId;
            ApplyBackpackBonus(player, item, adding: true);

            return true;
        }

        /// <summary>Tháo đồ ở slot về túi. False nếu slot trống hoặc túi không đủ chỗ chứa lại.</summary>
        public static bool TryUnequip(PlayerState player, DefinitionRegistry definitions, EquipSlot slot)
        {
            if (!player.Equipped.TryGetValue(slot, out string itemId)) return false;
            if (!definitions.TryGetItem(itemId, out var item)) return false;

            // Bỏ bonus backpack TRƯỚC khi kiểm tra chỗ chứa — tháo dry_bag co capacity lại,
            // đúng ràng buộc thực tế (không thể giữ nguyên sức chứa lớn sau khi cởi ra).
            ApplyBackpackBonus(player, item, adding: false);

            if (!InventorySystem.CanAdd(player.Inventory, definitions, definitions.Balance.Inventory, itemId, 1))
            {
                ApplyBackpackBonus(player, item, adding: true); // hoàn tác, tháo thất bại
                return false;
            }

            player.Equipped.Remove(slot);
            InventoryOps.AddItem(player.Inventory, definitions, itemId, 1);
            return true;
        }

        static void ApplyBackpackBonus(PlayerState player, ItemDefinition item, bool adding)
        {
            if (item.Protection == null) return;

            float sign = adding ? 1f : -1f;
            player.Inventory.CapacityKg += sign * item.Protection.BackpackCapacityKg;
            player.Inventory.CapacityLiters += sign * item.Protection.BackpackCapacityLiters;
        }

        public static float ComputeWetMultiplier(PlayerState player, DefinitionRegistry definitions) =>
            GetProtection(player, definitions, EquipSlot.Body)?.WetMultiplier ?? 1f;

        public static (int blockLevel, float mediumMultiplier) ComputeBootsProtection(
            PlayerState player, DefinitionRegistry definitions)
        {
            var protection = GetProtection(player, definitions, EquipSlot.Feet);
            return protection != null
                ? (protection.ExposureBlockLevel, protection.ExposureMediumMultiplier)
                : (0, 1f);
        }

        public static int ComputeCurrentReduction(PlayerState player, DefinitionRegistry definitions) =>
            GetProtection(player, definitions, EquipSlot.Tool)?.CurrentReduction ?? 0;

        static ItemProtection GetProtection(PlayerState player, DefinitionRegistry definitions, EquipSlot slot)
        {
            if (!player.Equipped.TryGetValue(slot, out string itemId)) return null;
            return definitions.TryGetItem(itemId, out var item) ? item.Protection : null;
        }
    }
}
