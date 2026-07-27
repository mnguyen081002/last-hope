using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Systems.Equipment;
using LastHope.Systems.Hazard;
using LastHope.Systems.Inventory;

namespace LastHope.Systems.Travel
{
    public static class TravelSystem
    {
        /// <summary>Số phút thực tế sẽ tốn nếu đi route này ngay bây giờ (đã nhân loadFactor × floodTimeFactor).</summary>
        public static int ComputeTravelMinutes(WorldState world, DefinitionRegistry definitions, string routeId)
        {
            var route = definitions.GetRoute(routeId);
            var balance = definitions.Balance;

            var tier = InventorySystem.ComputeLoadTier(world.Player.Inventory, definitions, balance.Inventory);
            float loadFactor = tier switch
            {
                LoadTier.Heavy or LoadTier.Blocked => balance.Travel.LoadFactorHeavy,
                LoadTier.Light => balance.Travel.LoadFactorLight,
                _ => balance.Travel.LoadFactorNormal,
            };

            var flood = EffectiveFlood(world, definitions, routeId);
            float floodTimeFactor = HazardSystem.IsPassable(flood)
                ? HazardSystem.TimeFactor(flood, balance.Hazard)
                : 1f; // Impassable chặn ở Command.Validate, không tới đây — giá trị này không dùng thực tế.

            return UnityEngine.Mathf.RoundToInt(route.TravelMinutes * loadFactor * floodTimeFactor);
        }

        static FloodState EffectiveFlood(WorldState world, DefinitionRegistry definitions, string routeId)
        {
            var route = definitions.GetRoute(routeId);
            var state = world.GetOrCreateRoute(routeId);
            var phase = DisasterPhaseSystem.CurrentPhase(world.WorldTimeMinutes, definitions.Balance.DisasterPhase);

            return HazardSystem.EffectiveFlood(route, state, phase);
        }

        /// <summary>
        /// Di chuyển: bơm thời gian qua route rồi đổi location. Không đụng scene — Presentation
        /// nghe <see cref="LastHope.Core.Events.LocationChanged"/> để load scene tương ứng.
        /// </summary>
        public static void Travel(
            WorldState world, DefinitionRegistry definitions, TickScheduler ticks, RngStream hazardRng, string routeId)
        {
            var route = definitions.GetRoute(routeId);
            string fromLocationId = world.Player.CurrentLocationId;
            string toLocationId = route.OtherEnd(fromLocationId);

            int minutes = ComputeTravelMinutes(world, definitions, routeId);
            ticks.FastForward(minutes);

            var player = world.Player;
            var balance = definitions.Balance;

            // Cộng một lần ngoài tick thường — chi phí của MỘT chuyến đi, không phải mỗi phút.
            player.Fatigue = UnityEngine.Mathf.Clamp(player.Fatigue + balance.Condition.FatiguePerTravel, 0f, 100f);

            var routeState = world.GetOrCreateRoute(routeId);
            var flood = EffectiveFlood(world, definitions, routeId);

            float wetMultiplier = EquipmentSystem.ComputeWetMultiplier(player, definitions);
            var (bootsBlockLevel, bootsMediumMultiplier) = EquipmentSystem.ComputeBootsProtection(player, definitions);
            int currentReduction = EquipmentSystem.ComputeCurrentReduction(player, definitions);

            HazardSystem.ApplyCrossingCost(
                player, flood, balance.Hazard, wetMultiplier, bootsBlockLevel, bootsMediumMultiplier);
            HazardSystem.ApplyCurrentCrossing(player, routeState.Current, balance.Hazard, hazardRng, currentReduction);
            HazardSystem.ApplyElectrifiedCrossing(player, routeState.IsElectrified, balance.Hazard, balance.Condition);

            player.CurrentLocationId = toLocationId;
        }
    }
}
