using System.Collections.Generic;
using LastHope.Data.Definitions;

namespace LastHope.Core.State
{
    public class PlayerState
    {
        public string CurrentLocationId;

        /// <summary>Vị trí trong scene hiện tại (world X/Y, 2D).</summary>
        public float PositionX;
        public float PositionY;

        public InventoryState Inventory = new();

        /// <summary>Item đang mặc theo slot. Slot chưa có entry = trống. Item mặc không nằm trong Inventory.Slots.</summary>
        public Dictionary<EquipSlot, string> Equipped = new();

        // Condition (P2-A). Xem docs/plans/2026-07-27-p2a-condition-core.md — Injury/
        // Disoriented chưa làm vì balance.json chưa có số cho hai nhóm đó.
        public float Health = 100f;
        public float Stamina = 100f;
        public float Fatigue;
        public float Hunger;
        public float Thirst;
        public float BodyTemperature = 37f;
        public float Wet;
        public float BlackWaterExposure;

        /// <summary>Hysteresis theo BodyTemperature — bật ở ColdBodyTempThreshold, tắt ở ColdClearBodyTempThreshold.</summary>
        public bool IsCold;

        /// <summary>Bật khi BlackWaterExposure vượt ngưỡng; không tự tắt (cần Shelter treat — P3).</summary>
        public bool IsSick;
    }
}
