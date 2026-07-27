namespace LastHope.Core.Time
{
    /// <summary>Quy đổi giữa <c>WorldTimeMinutes</c> và ngày/giờ hiển thị.</summary>
    public static class GameTimeUtil
    {
        public const int MinutesPerHour = 60;
        public const int MinutesPerDay = 24 * MinutesPerHour;

        /// <summary>Mốc gốc: phút 0 = Day 0, 17:00.</summary>
        public const int AnchorHour = 17;

        static long AbsoluteMinutes(long worldTimeMinutes) =>
            worldTimeMinutes + AnchorHour * MinutesPerHour;

        public static int DayIndex(long worldTimeMinutes) =>
            (int)(AbsoluteMinutes(worldTimeMinutes) / MinutesPerDay);

        public static int HourOfDay(long worldTimeMinutes) =>
            (int)(AbsoluteMinutes(worldTimeMinutes) % MinutesPerDay / MinutesPerHour);

        public static int MinuteOfHour(long worldTimeMinutes) =>
            (int)(AbsoluteMinutes(worldTimeMinutes) % MinutesPerHour);

        /// <summary>Dạng "Day 2 08:30".</summary>
        public static string Format(long worldTimeMinutes) =>
            $"Day {DayIndex(worldTimeMinutes)} " +
            $"{HourOfDay(worldTimeMinutes):00}:{MinuteOfHour(worldTimeMinutes):00}";
    }
}
