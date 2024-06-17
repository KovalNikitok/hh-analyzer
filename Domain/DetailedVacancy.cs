using System.Text.Json.Serialization;

namespace hh_analyzer.Domain
{
    public class DetailedVacancy
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("key_skills")]
        public List<Skill>? Skills { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }
    }
}
