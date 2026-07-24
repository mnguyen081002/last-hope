using System;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Rules
{
    /// <summary>
    /// Pure, deterministic inventory capacity/overload calculations — no Unity, no state
    /// mutation. Commands and Systems.Inventory both call this so the rule lives in one place.
    /// </summary>
    public static class InventoryRules
    {
        /// <summary>Only the player's own carried inventory is capacity-limited in P1/P2 —
        /// search points, shelter storage, and dropped-item piles are unlimited containers.</summary>
        public static bool IsCapacityLimited(string ownerId) => ownerId == "player";

        /// <summary>Backpack capacity, overridden by an equipped dry bag (ItemDefinition.Protection
        /// "backpack_capacity_kg"/"backpack_capacity_liters", S8) if one is worn in the "back"
        /// slot. definitions may be null (existing callers/tests that don't care about equipment
        /// keep the plain balance defaults).</summary>
        public static (float weightKg, float volumeLiters) EffectiveCapacity(
            InventoryState inventory, DefinitionRegistry definitions, BalanceConfig balance)
        {
            InventoryBalance cap = balance.Inventory;
            float weightCap = cap.BackpackCapacityKg;
            float volumeCap = cap.BackpackCapacityLiters;

            if (definitions != null
                && inventory.EquipmentSlots.TryGetValue("back", out string backInstanceId)
                && inventory.Items.TryGetValue(backInstanceId, out var backItem)
                && definitions.TryGetItem(backItem.ItemId, out var backDef)
                && backDef.Protection.TryGetValue("backpack_capacity_kg", out float overrideKg))
            {
                weightCap = overrideKg;
                volumeCap = backDef.Protection.TryGetValue("backpack_capacity_liters", out float overrideL) ? overrideL : volumeCap;
            }

            return (weightCap, volumeCap);
        }

        public static OverloadState ComputeOverload(InventoryState inventory, BalanceConfig balance, DefinitionRegistry definitions = null)
        {
            var (weightCap, volumeCap) = EffectiveCapacity(inventory, definitions, balance);
            float weightRatio = weightCap > 0 ? inventory.CurrentWeightKg / weightCap : 0f;
            float volumeRatio = volumeCap > 0 ? inventory.CurrentVolumeLiters / volumeCap : 0f;
            float ratio = Math.Max(weightRatio, volumeRatio);

            InventoryBalance cap = balance.Inventory;
            if (ratio > cap.OverloadHeavyThreshold) return OverloadState.Heavy;
            if (ratio > cap.OverloadLightThreshold) return OverloadState.Light;
            return OverloadState.Normal;
        }

        /// <summary>True if adding quantity of itemId would stay within the hard cap (150%).
        /// Non-capacity-limited destinations (search points, storage) always accept.</summary>
        public static bool CanAccept(
            InventoryState destination, DefinitionRegistry definitions, BalanceConfig balance,
            string itemId, int quantity)
        {
            if (!IsCapacityLimited(destination.OwnerId)) return true;
            if (!definitions.TryGetItem(itemId, out var def)) return false;

            var (weightCap, volumeCap) = EffectiveCapacity(destination, definitions, balance);
            float projectedWeight = destination.CurrentWeightKg + def.BaseWeightKg * quantity;
            float projectedVolume = destination.CurrentVolumeLiters + def.BaseVolumeLiters * quantity;

            return projectedWeight <= weightCap * balance.Inventory.HardCapMultiplier
                && projectedVolume <= volumeCap * balance.Inventory.HardCapMultiplier;
        }

        public static float SpeedModifierFor(OverloadState overload, BalanceConfig balance)
        {
            switch (overload)
            {
                case OverloadState.Light: return balance.Inventory.SpeedModifierLight;
                case OverloadState.Heavy: return balance.Inventory.SpeedModifierHeavy;
                default: return 1f;
            }
        }

        /// <summary>Travel time multiplier for the given carry state (BL-P1-19).</summary>
        public static float LoadFactorFor(OverloadState overload, BalanceConfig balance)
        {
            switch (overload)
            {
                case OverloadState.Light: return balance.Travel.LoadFactorLight;
                case OverloadState.Heavy: return balance.Travel.LoadFactorHeavy;
                default: return balance.Travel.LoadFactorNormal;
            }
        }
    }
}
