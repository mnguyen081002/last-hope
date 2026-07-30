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

        /// <summary>Kích thước footprint hộp ở hướng mặc định r000, theo world unit/grid cell.</summary>
        public float FootprintWidth = 1f;
        public float FootprintHeight = 1f;

        /// <summary>Chỉ bật khi Module có đủ sprite r000/r090/r180/r270.</summary>
        public bool IsRotatable;

        /// <summary>Bán kính legacy dùng cho vùng hover; placement dùng Width × Height.</summary>
        public float FootprintRadius = 0.5f;

        /// <summary>ItemId của bản "đã gói gọn" — Tháo Module tạo ra 1 cái, đặt lại tức thì
        /// không tốn Materials/BuildMinutes (xem BuildSystem.RedeployModule).</summary>
        public string PackedItemId;
    }
}
