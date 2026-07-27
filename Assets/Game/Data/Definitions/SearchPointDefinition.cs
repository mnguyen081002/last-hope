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

        /// <summary>
        /// Số giây thực phải giữ phím Interact mới mở được (0 = mở tức thì khi nhấn).
        /// Đại diện loại thao tác mở (mở tay không vs cạy khóa) — lever tune qua JSON.
        /// </summary>
        public float OpenHoldSeconds;

        public List<LootTableEntry> LootTable = new();
    }
}
