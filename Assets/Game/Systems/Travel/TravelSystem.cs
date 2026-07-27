using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
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

            var flood = world.GetOrCreateRoute(routeId).Flood;
            float floodTimeFactor = HazardSystem.IsPassable(flood)
                ? HazardSystem.TimeFactor(flood, balance.Hazard)
                : 1f; // Impassable chặn ở Command.Validate, không tới đây — giá trị này không dùng thực tế.

            return UnityEngine.Mathf.RoundToInt(route.TravelMinutes * loadFactor * floodTimeFactor);
        }

        /// <summary>
        /// Di chuyển: bơm thời gian qua route rồi đổi location. Không đụng scene — Presentation
        /// nghe <see cref="LastHope.Core.Events.LocationChanged"/> để load scene tương ứng.
        /// </summary>
        public static void Travel(
            WorldState world, DefinitionRegistry definitions, TickScheduler ticks, string routeId)
        {
            var route = definitions.GetRoute(routeId);
            string fromLocationId = world.Player.CurrentLocationId;
            string toLocationId = route.OtherEnd(fromLocationId);

            int minutes = ComputeTravelMinutes(world, definitions, routeId);
            ticks.FastForward(minutes);

            var player = world.Player;

            // Cộng một lần ngoài tick thường — chi phí của MỘT chuyến đi, không phải mỗi phút.
            player.Fatigue = UnityEngine.Mathf.Clamp(
                player.Fatigue + definitions.Balance.Condition.FatiguePerTravel, 0f, 100f);

            var flood = world.GetOrCreateRoute(routeId).Flood;
            HazardSystem.ApplyCrossingCost(player, flood, definitions.Balance.Hazard);

            player.CurrentLocationId = toLocationId;
        }
    }
}
