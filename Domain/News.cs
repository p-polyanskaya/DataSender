using System.Text.Json.Serialization;

namespace Domain;

public class News
{
    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }
    //public string Title { get; set; }
    public object Creator { get; set; }
    public object Content { get; set; }
    //public string PubDate { get; set; }
    public string? Description { get; set; }
}