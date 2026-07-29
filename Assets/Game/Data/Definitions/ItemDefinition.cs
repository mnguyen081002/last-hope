using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    public enum ItemCategory
    {
        Material,
        Water,
        Food,
        Equipment,
        Medical,
        Module,
    }

    public enum EquipSlot
    {
        None,
        Body,
        Feet,
        Hands,
        Tool,
        Back,
    }

    /// <summary>Hiệu ứng khi dùng item. Giá trị âm = giảm chỉ số (nước/đói), dương = hồi.</summary>
    public class ItemUseEffects
    {
        public float Thirst;
        public float Hunger;
        public float Health;
    }

    /// <summary>Chỉ số phòng hộ của equipment. Mặc định = không có tác dụng.</summary>
    public class ItemProtection
    {
        public float WetMultiplier = 1f;
        public int ExposureBlockLevel;
        public float ExposureMediumMultiplier = 1f;
        public int HandlesContaminated;
        public int CurrentReduction;
        public float BackpackCapacityKg;
        public float BackpackCapacityLiters;
    }

    public class ItemDefinition : DefinitionBase
    {
        public ItemCategory Category;
        public float BaseWeightKg;
        public float BaseVolumeLiters;
        public int MaxStackSize = 1;

        /// <summary>Vật cồng kềnh phải ôm 2 tay, không vào backpack.</summary>
        public bool TwoHandCarry;

        public EquipSlot EquipSlot = EquipSlot.None;
        public ItemUseEffects UseEffects;
        public ItemProtection Protection;
        public List<string> Tags = new();

        public bool IsStackable => MaxStackSize > 1;
        public bool IsEquipment => EquipSlot != EquipSlot.None;
    }
}
