using System.Text.Json.Serialization;

namespace TestApi.Models;

public class RequestModel
{
    [JsonPropertyName("selector")]
    public string? Selector { get; set; }

    [JsonPropertyName("attribute")]
    public string? Attribute { get; set; }

    [JsonPropertyName("url_b64")]
    public string? Url_b64 { get; set; }

    [JsonPropertyName("encrypted_text_bytes_b64")]
    public string? Encrypted_text_bytes_b64 { get; set; }

    [JsonPropertyName("key_bytes_b64")]
    public string? Key_bytes_b64 { get; set; }

    [JsonPropertyName("page_b64")]
    public string? Page_b64 { get; set; }
}
