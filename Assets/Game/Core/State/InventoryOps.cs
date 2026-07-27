using System.Collections.Generic;
using LastHope.Data;

namespace LastHope.Core.State
{
    /// <summary>
    /// Thao tác cơ bản trên danh sách <see cref="ItemInstanceState"/> — thuần tính toán,
    /// không có luật gameplay (overload, hai tay...). Luật đầy đủ nằm ở InventorySystem.
    ///
    /// Nhận thẳng <see cref="List{ItemInstanceState}"/> thay vì <see cref="InventoryState"/>
    /// để dùng chung cho player, shelter storage và search point remaining items — ba nơi
    /// đó không phải lúc nào cũng có sức chứa (storage/search point không giới hạn).
    /// </summary>
    public static class InventoryOps
    {
        public static float TotalWeightKg(List<ItemInstanceState> slots, DefinitionRegistry definitions)
        {
            float total = 0f;
            foreach (var slot in slots)
            {
                if (definitions.TryGetItem(slot.ItemId, out var definition))
                    total += definition.BaseWeightKg * slot.Quantity;
            }
            return total;
        }

        public static float TotalVolumeLiters(List<ItemInstanceState> slots, DefinitionRegistry definitions)
        {
            float total = 0f;
            foreach (var slot in slots)
            {
                if (definitions.TryGetItem(slot.ItemId, out var definition))
                    total += definition.BaseVolumeLiters * slot.Quantity;
            }
            return total;
        }

        public static int CountOf(List<ItemInstanceState> slots, string itemId)
        {
            int total = 0;
            foreach (var slot in slots)
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
            List<ItemInstanceState> slots, DefinitionRegistry definitions,
            string itemId, int quantity)
        {
            if (quantity <= 0) return;

            int maxStack = definitions.TryGetItem(itemId, out var definition)
                ? definition.MaxStackSize
                : 1;

            int remaining = quantity;

            foreach (var slot in slots)
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
                slots.Add(new ItemInstanceState { ItemId = itemId, Quantity = amount });
                remaining -= amount;
            }
        }

        /// <summary>Bớt đồ. Trả về số thực sự bớt được (có thể ít hơn yêu cầu).</summary>
        public static int RemoveItem(List<ItemInstanceState> slots, string itemId, int quantity)
        {
            if (quantity <= 0) return 0;

            int remaining = quantity;

            for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var slot = slots[i];
                if (slot.ItemId != itemId) continue;

                int taken = slot.Quantity < remaining ? slot.Quantity : remaining;
                slot.Quantity -= taken;
                remaining -= taken;

                if (slot.Quantity <= 0) slots.RemoveAt(i);
            }

            return quantity - remaining;
        }

        /// <summary>
        /// Chuyển tối đa <paramref name="quantity"/> từ <paramref name="from"/> sang
        /// <paramref name="to"/>. Trả về số thực sự chuyển được — caller quyết định coi đó
        /// là partial-success hay fail tùy ngữ cảnh (vd. Take All báo triage khi &lt; yêu cầu).
        /// </summary>
        public static int Move(
            List<ItemInstanceState> from, List<ItemInstanceState> to,
            DefinitionRegistry definitions, string itemId, int quantity)
        {
            int available = CountOf(from, itemId);
            int moved = quantity < available ? quantity : available;
            if (moved <= 0) return 0;

            RemoveItem(from, itemId, moved);
            AddItem(to, definitions, itemId, moved);
            return moved;
        }

        // ---------- Overload tiện dùng cho player inventory (InventoryState) ----------

        public static float TotalWeightKg(InventoryState inventory, DefinitionRegistry definitions) =>
            TotalWeightKg(inventory.Slots, definitions);

        public static float TotalVolumeLiters(InventoryState inventory, DefinitionRegistry definitions) =>
            TotalVolumeLiters(inventory.Slots, definitions);

        public static int CountOf(InventoryState inventory, string itemId) =>
            CountOf(inventory.Slots, itemId);

        public static void AddItem(
            InventoryState inventory, DefinitionRegistry definitions, string itemId, int quantity) =>
            AddItem(inventory.Slots, definitions, itemId, quantity);

        public static int RemoveItem(InventoryState inventory, string itemId, int quantity) =>
            RemoveItem(inventory.Slots, itemId, quantity);
    }
}
