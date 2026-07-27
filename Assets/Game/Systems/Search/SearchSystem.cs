using System.Collections.Generic;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Inventory;

namespace LastHope.Systems.Search
{
    /// <summary>
    /// Search Point = container mở tức thì, thấy toàn bộ ngay (thiết kế chốt 2026-07-24).
    /// Nội dung roll một lần lúc mở đầu tiên; đồ không lấy nằm lại vĩnh viễn.
    /// </summary>
    public static class SearchSystem
    {
        /// <summary>Mở search point — roll nếu là lần đầu, ngược lại trả về state đã có.</summary>
        public static SearchPointState Open(
            WorldState world, DefinitionRegistry definitions, RngService rng, string searchPointId)
        {
            var definition = definitions.GetSearchPoint(searchPointId);
            var location = world.GetOrCreateLocation(definition.LocationId);

            if (!location.SearchPoints.TryGetValue(searchPointId, out var state))
            {
                state = new SearchPointState();
                location.SearchPoints[searchPointId] = state;
            }

            if (!state.Rolled)
            {
                Roll(state, definition, definitions, rng.Stream(RngService.Loot));
                state.Rolled = true;
            }

            return state;
        }

        static void Roll(
            SearchPointState state, SearchPointDefinition definition,
            DefinitionRegistry definitions, RngStream stream)
        {
            foreach (var entry in definition.LootTable)
            {
                bool include = entry.Guaranteed || stream.NextChance(entry.Chance);
                if (!include) continue;

                int quantity = stream.NextIntInclusive(entry.MinQuantity, entry.MaxQuantity);
                if (quantity <= 0) continue;

                InventoryOps.AddItem(state.RemainingItems, definitions, entry.ItemId, quantity);
            }
        }

        /// <summary>
        /// Lấy hết mọi thứ còn lại, giới hạn bởi sức chứa player. Trả về true nếu lấy được
        /// **toàn bộ** — false nghĩa là còn sót lại (triage), gọi nơi để biết mà báo UI.
        /// </summary>
        public static bool TakeAll(
            WorldState world, DefinitionRegistry definitions, string searchPointId)
        {
            var definition = definitions.GetSearchPoint(searchPointId);
            var location = world.GetOrCreateLocation(definition.LocationId);
            if (!location.SearchPoints.TryGetValue(searchPointId, out var state)) return true;

            var balance = definitions.Balance.Inventory;
            var inventory = world.Player.Inventory;
            bool tookEverything = true;

            // Duyệt bản sao ID vì Move có thể xóa slot khỏi RemainingItems khi lấy hết.
            var itemIds = new List<string>();
            foreach (var slot in state.RemainingItems) itemIds.Add(slot.ItemId);

            foreach (string itemId in itemIds)
            {
                int available = InventoryOps.CountOf(state.RemainingItems, itemId);
                if (available <= 0) continue;

                int allowed = InventorySystem.CanAdd(inventory, definitions, balance, itemId, available)
                    ? available
                    : MaxAddable(inventory, definitions, balance, itemId, available);

                if (allowed < available) tookEverything = false;
                if (allowed <= 0) continue;

                InventoryOps.Move(state.RemainingItems, inventory.Slots, definitions, itemId, allowed);
            }

            return tookEverything;
        }

        /// <summary>Tìm nhị phân số lượng lớn nhất còn nhặt được trong giới hạn hard cap.</summary>
        static int MaxAddable(
            InventoryState inventory, DefinitionRegistry definitions,
            InventoryBalance balance, string itemId, int upperBound)
        {
            int low = 0, high = upperBound;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (InventorySystem.CanAdd(inventory, definitions, balance, itemId, mid))
                    low = mid;
                else
                    high = mid - 1;
            }
            return low;
        }
    }
}
