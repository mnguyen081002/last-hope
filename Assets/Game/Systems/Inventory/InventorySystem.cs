using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Inventory
{
    public enum LoadTier
    {
        Normal,
        Light,
        Heavy,

        /// <summary>Vượt trần cứng — không nhặt thêm được.</summary>
        Blocked,
    }

    /// <summary>Luật overload/carried-object trên <see cref="InventoryState"/> của player.</summary>
    public static class InventorySystem
    {
        public static float LoadRatio(InventoryState inventory, DefinitionRegistry definitions)
        {
            if (!inventory.HasCapacityLimit) return 0f;

            float weightRatio = inventory.CapacityKg > 0f
                ? InventoryOps.TotalWeightKg(inventory, definitions) / inventory.CapacityKg
                : 0f;
            float volumeRatio = inventory.CapacityLiters > 0f
                ? InventoryOps.TotalVolumeLiters(inventory, definitions) / inventory.CapacityLiters
                : 0f;

            return weightRatio > volumeRatio ? weightRatio : volumeRatio;
        }

        public static LoadTier ComputeLoadTier(
            InventoryState inventory, DefinitionRegistry definitions, InventoryBalance balance)
        {
            float ratio = LoadRatio(inventory, definitions);

            if (ratio >= balance.HardCapMultiplier) return LoadTier.Blocked;
            if (ratio >= balance.OverloadHeavyThreshold) return LoadTier.Heavy;
            if (ratio >= balance.OverloadLightThreshold) return LoadTier.Light;
            return LoadTier.Normal;
        }

        public static float SpeedModifierFor(LoadTier tier, InventoryBalance balance) => tier switch
        {
            LoadTier.Heavy => balance.SpeedModifierHeavy,
            LoadTier.Blocked => balance.SpeedModifierHeavy,
            LoadTier.Light => balance.SpeedModifierLight,
            _ => 1f,
        };

        /// <summary>
        /// Còn nhặt được <paramref name="itemId"/> ×<paramref name="quantity"/> không.
        /// Vật cồng kềnh (two-hand carry) không tính vào backpack — chiếm
        /// <see cref="InventoryState.CarriedObjectItemId"/>, chỉ giữ được 1 cái.
        /// </summary>
        public static bool CanAdd(
            InventoryState inventory, DefinitionRegistry definitions,
            InventoryBalance balance, string itemId, int quantity)
        {
            if (quantity <= 0) return false;
            if (!definitions.TryGetItem(itemId, out var item)) return false;

            if (item.TwoHandCarry)
            {
                return string.IsNullOrEmpty(inventory.CarriedObjectItemId) && quantity == 1;
            }

            if (!inventory.HasCapacityLimit) return true;

            float projectedWeight = InventoryOps.TotalWeightKg(inventory, definitions)
                + item.BaseWeightKg * quantity;
            float projectedVolume = InventoryOps.TotalVolumeLiters(inventory, definitions)
                + item.BaseVolumeLiters * quantity;

            float hardCapKg = inventory.CapacityKg * balance.HardCapMultiplier;
            float hardCapLiters = inventory.CapacityLiters * balance.HardCapMultiplier;

            bool overWeight = inventory.CapacityKg > 0f && projectedWeight > hardCapKg;
            bool overVolume = inventory.CapacityLiters > 0f && projectedVolume > hardCapLiters;

            return !overWeight && !overVolume;
        }

        /// <summary>
        /// Thêm đồ theo đúng luật (carried object tách khỏi backpack). Caller phải
        /// <see cref="CanAdd"/> trước — hàm này không kiểm tra lại sức chứa.
        /// </summary>
        public static void Add(
            InventoryState inventory, DefinitionRegistry definitions, string itemId, int quantity)
        {
            if (definitions.TryGetItem(itemId, out var item) && item.TwoHandCarry)
            {
                inventory.CarriedObjectItemId = itemId;
                return;
            }

            InventoryOps.AddItem(inventory, definitions, itemId, quantity);
        }
    }
}
