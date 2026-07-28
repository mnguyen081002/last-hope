using System.Collections.Generic;

namespace LastHope.Core.State
{
    public enum PowerPriority { Critical, High, Normal, Disabled }

    /// <summary>Trạng thái một Module đã xây xong, gắn vào một Build Slot cụ thể.</summary>
    public class BuiltModuleState
    {
        public string ModuleId;
        public float Durability = 100f;
        public PowerPriority Priority = PowerPriority.Normal;
        public bool IsJammed;

        /// <summary>Kết quả PowerSystem tick gần nhất — không phải nguồn sự thật, chỉ để UI đọc.</summary>
        public bool Powered;
    }

    /// <summary>Công trình đang xây dở tại một Build Slot — chỉ một cái chạy cùng lúc (MVP).</summary>
    public class ConstructionState
    {
        public string SlotId;
        public string ModuleId;
        public float MinutesRemaining;
        public bool Paused;
    }

    /// <summary>
    /// Trạng thái Main Shelter (chỉ một shelter trong MVP — không key theo LocationId).
    /// Vật liệu xây dựng và Storage vẫn dùng <see cref="LocationState.StorageContainer"/> sẵn
    /// có (P1) — ShelterState chỉ giữ phần P3 mới: Water Intrusion, Power, Water Processing,
    /// Build Slot, Event flag.
    /// </summary>
    public class ShelterState
    {
        public float StructuralIntegrity = 85f;
        public float WaterIntrusion;
        public float CleanWater;
        public float UntreatedWater;
        public float BatteryCharge;

        /// <summary>Phút đã tích luỹ cho batch Water Purifier hiện tại (reset về 0 khi hoàn thành batch).</summary>
        public float PurifierBatchMinutes;
        public float PurifierFilterDurability = 100f;

        public bool DrainBackflowActive;
        public bool StorageFloodRiskActive;

        public Dictionary<string, BuiltModuleState> BuildSlots = new();
        public ConstructionState Construction;
    }
}
