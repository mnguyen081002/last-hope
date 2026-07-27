using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    /// <summary>
    /// Một dòng loot. <see cref="Guaranteed"/> = luôn có; ngược lại roll theo
    /// <see cref="Chance"/> (phần trăm 0–100).
    /// </summary>
    public class LootTableEntry
    {
        public string ItemId;
        public bool Guaranteed;
        public float Chance;
        public int MinQuantity = 1;
        public int MaxQuantity = 1;
    }

    /// <summary>
    /// Search Point là container: mở ra thấy toàn bộ nội dung. Nội dung roll một lần duy
    /// nhất lúc mở lần đầu, phần không lấy nằm lại vĩnh viễn.
    /// </summary>
    public class SearchPointDefinition : DefinitionBase
    {
        public string LocationId;

        /// <summary>0 = mở tức thì. Là lever tune qua JSON, không sửa code.</summary>
        public int OpenTimeMinutes;

        public List<LootTableEntry> LootTable = new();
    }
}
