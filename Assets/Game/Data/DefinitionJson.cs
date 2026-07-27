using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace LastHope.Data
{
    /// <summary>
    /// Cấu hình JSON dùng chung cho Definition. PascalCase trong C# ↔ snake_case trên đĩa.
    /// Một nơi duy nhất định nghĩa — đổi ở đây là đổi toàn bộ pipeline đọc definition.
    /// </summary>
    public static class DefinitionJson
    {
        public static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy
                {
                    // Key của dictionary là ID do content đặt, không đụng vào.
                    ProcessDictionaryKeys = false,
                },
            },
            Converters = { new StringEnumConverter(new SnakeCaseNamingStrategy()) },
            TypeNameHandling = TypeNameHandling.None,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Ignore,
            Culture = CultureInfo.InvariantCulture,
        };

        public static T Deserialize<T>(string json) =>
            JsonConvert.DeserializeObject<T>(json, Settings);
    }
}
