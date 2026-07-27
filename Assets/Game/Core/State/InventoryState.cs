using System.Collections.Generic;

namespace LastHope.Core.State
{
    /// <summary>
    /// Một túi đồ bất kỳ: backpack người chơi, container trong shelter, hay đồ còn lại
    /// trong một search point. Sức chứa 0 = không giới hạn (dùng cho kho shelter).
    /// </summary>
    public class InventoryState
    {
        public List<ItemInstanceState> Slots = new();

        public float CapacityKg;
        public float CapacityLiters;

        /// <summary>Vật cồng kềnh phải ôm 2 tay, nằm ngoài backpack.</summary>
        public string CarriedObjectItemId;

        public bool HasCapacityLimit => CapacityKg > 0f || CapacityLiters > 0f;
    }
}
