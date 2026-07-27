using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Systems.Inventory;

namespace LastHope.Systems.Travel
{
    public static class TravelSystem
    {
        /// <summary>Số phút thực tế sẽ tốn nếu đi route này ngay bây giờ (đã nhân loadFactor).</summary>
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

            return UnityEngine.Mathf.RoundToInt(route.TravelMinutes * loadFactor);
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

            world.Player.CurrentLocationId = toLocationId;
        }
    }
}
