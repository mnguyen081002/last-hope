using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    public enum ShelterFloor { Ground, Upper, Roof }

    public enum ShelterWaterRisk { None, Low, Medium, High, Critical }

    /// <summary>Zone trong Main Shelter, chứa Build Slot. Khớp <c>shelterzones_p3.json</c>.</summary>
    public class ShelterZoneDefinition : DefinitionBase
    {
        public ShelterFloor Floor;
        public List<string> BuildSlotIds = new();
        public ShelterWaterRisk WaterRisk;
    }
}
