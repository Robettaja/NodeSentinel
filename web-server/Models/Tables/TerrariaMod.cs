using System.Text.Json.Serialization;
using web_server.Models.Tables;

namespace server.Models.Tables
{

    public class TerrariaMod : SaveableObject 
    {
        [property: JsonPropertyName("publishedfileid")]
        public string? ModId { get; set; }
        [property: JsonPropertyName("title")]
        public string? Title { get; set; }
        [property: JsonPropertyName("preview_url")]
        public string? PreviewUrl { get; set; }

        [property: JsonPropertyName("kvtags")]
        public List<KvTag>? KvTags { get; set; }

        [property: JsonPropertyName("file_size")]
        public string ModSize { get; set; }

        [property: JsonPropertyName("views")]
        public int Views { get; set; }

        [property: JsonPropertyName("time_updated")]
        public int TimeUpdated { get; set; }
    }
}