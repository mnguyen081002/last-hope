using LastHope.Data;

namespace LastHope.Core.State
{
    /// <summary>
    /// Thao tác cơ bản trên <see cref="InventoryState"/> — thuần tính toán, không có luật
    /// gameplay (overload, hai tay...). Luật đầy đủ nằm ở InventorySystem (S5).
    /// </summary>
    public static class InventoryOps
    {
        public static float TotalWeightKg(InventoryState inventory, DefinitionRegistry definitions)
        {
            float total = 0f;
            foreach (var slot in inventory.Slots)
            {
                if (definitions.TryGetItem(slot.ItemId, out var definition))
                    total += definition.BaseWeightKg * slot.Quantity;
            }
            return total;
        }

        public static float TotalVolumeLiters(InventoryState inventory, DefinitionRegistry definitions)
        {
            float total = 0f;
            foreach (var slot in inventory.Slots)
            {
                if (definitions.TryGetItem(slot.ItemId, out var definition))
                    total += definition.BaseVolumeLiters * slot.Quantity;
            }
            return total;
        }

        public static int CountOf(InventoryState inventory, string itemId)
        {
            int total = 0;
            foreach (var slot in inventory.Slots)
            {
                if (slot.ItemId == itemId) total += slot.Quantity;
            }
            return total;
        }

        /// <summary>
        /// Thêm đồ, gộp vào stack sẵn có trước rồi mới mở ngăn mới. Không kiểm tra sức
        /// chứa — caller (InventorySystem/command) chịu trách nhiệm validate trước.
        /// </summary>
        public static void AddItem(
            InventoryState inventory, DefinitionRegistry definitions,
            string itemId, int quantity)
        {
            if (quantity <= 0) return;

            int maxStack = definitions.TryGetItem(itemId, out var definition)
                ? definition.MaxStackSize
                : 1;

            int remaining = quantity;

            foreach (var slot in inventory.Slots)
            {
                if (remaining <= 0) break;
                if (slot.ItemId != itemId || slot.Quantity >= maxStack) continue;

                int room = maxStack - slot.Quantity;
                int moved = remaining < room ? remaining : room;
                slot.Quantity += moved;
                remaining -= moved;
            }

            while (remaining > 0)
            {
                int amount = remaining < maxStack ? remaining : maxStack;
                inventory.Slots.Add(new ItemInstanceState { ItemId = itemId, Quantity = amount });
                remaining -= amount;
            }
        }

        /// <summary>Bớt đồ. Trả về số thực sự bớt được (có thể ít hơn yêu cầu).</summary>
        public static int RemoveItem(InventoryState inventory, string itemId, int quantity)
        {
            if (quantity <= 0) return 0;

            int remaining = quantity;

            for (int i = inventory.Slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var slot = inventory.Slots[i];
                if (slot.ItemId != itemId) continue;

                int taken = slot.Quantity < remaining ? slot.Quantity : remaining;
                slot.Quantity -= taken;
                remaining -= taken;

                if (slot.Quantity <= 0) inventory.Slots.RemoveAt(i);
            }

            return quantity - remaining;
        }
    }
}
