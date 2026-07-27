using System.Globalization;
using LastHope.Core.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LastHope.Core.Save
{
    /// <summary>
    /// Serialize <see cref="WorldState"/>. Cấu hình phải ổn định tuyệt đối: đổi setting ở
    /// đây là làm hỏng mọi save cũ, nên mọi thay đổi phải đi kèm tăng save version.
    /// </summary>
    public static class WorldStateSerializer
    {
        public static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy
                {
                    // Key dictionary là ID content (location/rng stream), giữ nguyên.
                    ProcessDictionaryKeys = false,
                },
            },
            TypeNameHandling = TypeNameHandling.None,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Include,
            DateParseHandling = DateParseHandling.None,
            FloatParseHandling = FloatParseHandling.Double,
            Culture = CultureInfo.InvariantCulture,
            Formatting = Formatting.None,
        };

        public static string Serialize(WorldState state) =>
            JsonConvert.SerializeObject(state, Settings);

        public static WorldState Deserialize(string json) =>
            JsonConvert.DeserializeObject<WorldState>(json, Settings);
    }
}
