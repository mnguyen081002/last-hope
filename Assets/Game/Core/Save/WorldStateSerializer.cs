using LastHope.Core.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace LastHope.Core.Save
{
    /// <summary>
    /// Canonical Newtonsoft settings for WorldState (technical-specification.md mục 9/§32).
    /// Snake_case on disk, version-tolerant (missing/removed fields don't break load),
    /// no $type polymorphism, deterministic member/collection order for deep-compare tests.
    /// </summary>
    public static class WorldStateSerializer
    {
        public static JsonSerializerSettings Settings { get; } = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = false }
            },
            Converters = { new StringEnumConverter() },
            TypeNameHandling = TypeNameHandling.None,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Formatting = Formatting.Indented,
        };

        /// <summary>Human-readable (indented) — used for save files and the Debug Panel state dump.</summary>
        public static string Serialize(WorldState world) => JsonConvert.SerializeObject(world, Settings);

        /// <summary>Single-line — used for checksums and deep-compare in tests.</summary>
        public static string SerializeCanonical(WorldState world)
        {
            var canonical = new JsonSerializerSettings
            {
                ContractResolver = Settings.ContractResolver,
                Converters = Settings.Converters,
                TypeNameHandling = Settings.TypeNameHandling,
                MissingMemberHandling = Settings.MissingMemberHandling,
                NullValueHandling = Settings.NullValueHandling,
                DefaultValueHandling = Settings.DefaultValueHandling,
                ObjectCreationHandling = Settings.ObjectCreationHandling,
                Formatting = Formatting.None,
            };
            return JsonConvert.SerializeObject(world, canonical);
        }

        public static WorldState Deserialize(string json) => JsonConvert.DeserializeObject<WorldState>(json, Settings);
    }
}
