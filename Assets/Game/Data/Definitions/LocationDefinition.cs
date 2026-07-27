using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    public class LocationDefinition : DefinitionBase
    {
        public List<string> SearchPointIds = new();
        public List<string> ConnectedRouteIds = new();

        /// <summary>Tên scene gameplay tương ứng, dùng khi Travel load scene.</summary>
        public string SceneName;

        public bool IsShelter;
    }
}
