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

        public static OverloadState ComputeOverload(InventoryState inventory, BalanceConfig balance)
        {
            InventoryBalance cap = balance.Inventory;
            float weightRatio = cap.BackpackCapacityKg > 0 ? inventory.CurrentWeightKg / cap.BackpackCapacityKg : 0f;
            float volumeRatio = cap.BackpackCapacityLiters > 0 ? inventory.CurrentVolumeLiters / cap.BackpackCapacityLiters : 0f;
            float ratio = Math.Max(weightRatio, volumeRatio);

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

            InventoryBalance cap = balance.Inventory;
            float projectedWeight = destination.CurrentWeightKg + def.BaseWeightKg * quantity;
            float projectedVolume = destination.CurrentVolumeLiters + def.BaseVolumeLiters * quantity;

            return projectedWeight <= cap.BackpackCapacityKg * cap.HardCapMultiplier
                && projectedVolume <= cap.BackpackCapacityLiters * cap.HardCapMultiplier;
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
