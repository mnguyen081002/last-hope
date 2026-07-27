namespace LastHope.Core.State
{
    /// <summary>
    /// Một ngăn đồ. Stack chỉ gộp được khi cùng ItemId và cùng trạng thái phụ — nếu không,
    /// đồ hỏng sẽ gộp với đồ mới và mất thông tin.
    /// </summary>
    public class ItemInstanceState
    {
        public string ItemId;
        public int Quantity = 1;

        /// <summary>0–100. 100 = nguyên vẹn.</summary>
        public float Condition = 100f;

        /// <summary>0–100. &gt;0 = nhiễm bẩn, không uống/ăn trực tiếp được.</summary>
        public float Contamination;

        public bool CanStackWith(ItemInstanceState other) =>
            other != null
            && other.ItemId == ItemId
            && Approximately(other.Condition, Condition)
            && Approximately(other.Contamination, Contamination);

        static bool Approximately(float a, float b) => System.Math.Abs(a - b) < 0.001f;

        public ItemInstanceState Clone() => new()
        {
            ItemId = ItemId,
            Quantity = Quantity,
            Condition = Condition,
            Contamination = Contamination,
        };
    }
}
