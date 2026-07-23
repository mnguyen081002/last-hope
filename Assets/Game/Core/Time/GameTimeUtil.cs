namespace LastHope.Core.Time
{
    /// <summary>
    /// Derived time math from the canonical world_time_minutes counter (technical-specification.md
    /// mục 9/§7). Anchor: Day 0, 17:00 = minute 0.
    /// </summary>
    public static class GameTimeUtil
    {
        private const int AnchorMinutesOfDay = 17 * 60;
        private const int MinutesPerDay = 24 * 60;

        public static int DayIndex(long worldTimeMinutes) =>
            (int)((worldTimeMinutes + AnchorMinutesOfDay) / MinutesPerDay);

        public static int TimeOfDayMinutes(long worldTimeMinutes) =>
            (int)((worldTimeMinutes + AnchorMinutesOfDay) % MinutesPerDay);

        public static string Format(long worldTimeMinutes)
        {
            int day = DayIndex(worldTimeMinutes);
            int tod = TimeOfDayMinutes(worldTimeMinutes);
            return $"Day {day} {tod / 60:00}:{tod % 60:00}";
        }
    }
}
