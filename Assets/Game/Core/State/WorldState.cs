using System.Collections.Generic;

namespace LastHope.Core.State
{
    /// <summary>
    /// Toàn bộ trạng thái runtime của một ván chơi. Đây là thứ duy nhất được serialize vào
    /// save — không có state gameplay nào sống ngoài object này.
    /// </summary>
    public class WorldState
    {
        /// <summary>
        /// Thời gian canonical, tính bằng **phút** kể từ mốc Day 0 17:00. Chỉ
        /// <c>TickScheduler.AdvanceOneMinute</c> được phép tăng giá trị này.
        /// </summary>
        public long WorldTimeMinutes;

        public ulong MasterSeed;

        /// <summary>State của từng RNG stream đặt tên, để load xong chạy tiếp đúng bit.</summary>
        public Dictionary<string, ulong> RngStreams = new();

        public PlayerState Player = new();

        public Dictionary<string, LocationState> Locations = new();

        /// <summary>Route chưa từng đổi flood state thì không có entry (mặc định Dry).</summary>
        public Dictionary<string, RouteState> Routes = new();

        /// <summary>Chỉ một Main Shelter trong MVP (P3).</summary>
        public ShelterState Shelter = new();

        public LocationState GetOrCreateLocation(string locationId)
        {
            if (!Locations.TryGetValue(locationId, out var state))
            {
                state = new LocationState();
                Locations[locationId] = state;
            }
            return state;
        }

        public RouteState GetOrCreateRoute(string routeId)
        {
            if (!Routes.TryGetValue(routeId, out var state))
            {
                state = new RouteState();
                Routes[routeId] = state;
            }
            return state;
        }
    }

    /// <summary>Trạng thái đã thay đổi của một location. Location chưa từng tới thì không có entry.</summary>
    public class LocationState
    {
        public Dictionary<string, SearchPointState> SearchPoints = new();

        /// <summary>Đồ người chơi vứt xuống đất, nằm lại qua save/load.</summary>
        public List<ItemInstanceState> DroppedItems = new();

        /// <summary>
        /// Kho lưu trữ tại location (chỉ shelter dùng tới). Không giới hạn sức chứa — luật
        /// capacity chỉ áp cho backpack người chơi.
        /// </summary>
        public List<ItemInstanceState> StorageContainer = new();
    }

    /// <summary>
    /// Nội dung container roll **một lần duy nhất** lúc mở đầu tiên rồi ghi lại đây. Đồ
    /// không lấy nằm nguyên vĩnh viễn — đó là lý do quay lại một location cụ thể.
    /// </summary>
    public class SearchPointState
    {
        public bool Rolled;
        public List<ItemInstanceState> RemainingItems = new();
    }
}
