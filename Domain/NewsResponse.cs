namespace Domain;

public class NewsResponse
{
    public string Status { get; set; }
    public IReadOnlyCollection<News> Results { get; set; } = new List<News>();
}