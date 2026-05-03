using System.Text.Json.Serialization;

namespace web_server.Models.Tables;

public class KvTag
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}