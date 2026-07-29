using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    /// <summary>Module xây được trong Shelter, đặt tự do trong Zone (Free Placement). Khớp <c>modules_p3.json</c>.</summary>
    public class ModuleDefinition : DefinitionBase
    {
        public List<string> AllowedZoneIds = new();

        /// <summary>ItemId → số lượng tiêu thụ khi bắt đầu xây (Resource reservation).</summary>
        public Dictionary<string, int> Materials = new();

        public int BuildMinutes;
        public int PowerDemand;
        public float MaxDurability = 100f;

        /// <summary>Bán kính va chạm dùng để chặn đặt chồng lên Module khác — chưa có kích thước sprite thật.</summary>
        public float FootprintRadius = 0.5f;

        /// <summary>ItemId của bản "đã gói gọn" — Tháo Module tạo ra 1 cái, đặt lại tức thì
        /// không tốn Materials/BuildMinutes (xem BuildSystem.RedeployModule).</summary>
        public string PackedItemId;
    }
}
