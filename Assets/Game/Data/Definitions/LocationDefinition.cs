using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    public sealed class LocationDefinition : DefinitionBase
    {
        public List<string> SearchPointIds { get; set; } = new List<string>();
        public List<string> ConnectedRouteIds { get; set; } = new List<string>();
    }
}
