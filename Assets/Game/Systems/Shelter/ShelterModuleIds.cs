namespace LastHope.Systems.Shelter
{
    /// <summary>
    /// ID literal cho content P3 cố định. MVP chỉ có một Main Shelter và 5 Module — dùng
    /// literal trực tiếp giống quy ước P1/P2 (route/location id trong Command/SceneSetup),
    /// không cần tra cứu gián tiếp qua tag.
    /// </summary>
    public static class ShelterModuleIds
    {
        public const string LocationId = "location_shelter";

        public const string Barrier = "module_barrier";
        public const string Pump = "module_pump";
        public const string ElevatedStorage = "module_elevated_storage";
        public const string Purifier = "module_purifier";
        public const string BatteryBank = "module_battery_bank";
    }
}
