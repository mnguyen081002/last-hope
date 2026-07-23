namespace LastHope.Data.Definitions
{
    /// <summary>
    /// Common fields for every Definition type (technical-specification.md mục 9/§4):
    /// stable id, no runtime state, editable without code changes.
    /// </summary>
    public abstract class DefinitionBase
    {
        public string Id { get; set; }
        public string DisplayNameKey { get; set; }
        public int DataVersion { get; set; } = 1;
    }
}
