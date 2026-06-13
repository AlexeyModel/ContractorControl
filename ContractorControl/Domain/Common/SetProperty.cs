using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContractorControl.Domain.Common
{
    public class SetPropertyInfo
    {
        [JsonPropertyName("table_name")]
        public required string TableName { get; set; }
        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("secret_key")]
        public string SecretKey { get; set; } = string.Empty;
    }
}
