using System.Text.Json.Serialization;

namespace ContractorControl.Domain.Common
{
    public class SecretInfo
    {
        [JsonPropertyName("secret_key")]
        public string SecretKey { get; set; } = string.Empty;
    }
}
