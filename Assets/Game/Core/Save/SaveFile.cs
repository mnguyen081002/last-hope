using Newtonsoft.Json.Linq;

namespace LastHope.Core.Save
{
    /// <summary>On-disk save DTO (technical-specification.md mục 9/§29-32).</summary>
    public sealed class SaveFile
    {
        public int SaveVersion { get; set; } = 1;
        public string DefinitionVersion { get; set; }
        public string SavedAtUtc { get; set; }
        public string Checksum { get; set; }
        public string SlotId { get; set; }

        /// <summary>Embedded verbatim (JRaw): the exact canonical world payload the checksum covers.</summary>
        public JRaw World { get; set; }
    }

    public sealed class SaveSlotInfo
    {
        public string SlotId { get; set; }
        public string SavedAtUtc { get; set; }
        public string DefinitionVersion { get; set; }
    }
}
