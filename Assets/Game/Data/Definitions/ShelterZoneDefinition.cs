namespace LastHope.Data.Definitions
{
    public enum ShelterFloor { Ground, Upper, Roof }

    public enum ShelterWaterRisk { None, Low, Medium, High, Critical }

    /// <summary>
    /// Zone trong Main Shelter — vùng world tự do để đặt Module (Free Placement, thay Build
    /// Slot cố định — xem docs/plans/2026-07-28-free-placement.md). Khớp
    /// <c>shelterzones_p3.json</c>.
    /// </summary>
    public class ShelterZoneDefinition : DefinitionBase
    {
        public ShelterFloor Floor;
        public ShelterWaterRisk WaterRisk;

        public float BoundsMinX;
        public float BoundsMinY;
        public float BoundsMaxX;
        public float BoundsMaxY;

        public bool Contains(float x, float y) =>
            x >= BoundsMinX && x <= BoundsMaxX && y >= BoundsMinY && y <= BoundsMaxY;
    }
}
