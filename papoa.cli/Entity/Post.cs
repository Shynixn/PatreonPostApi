namespace Papoa.Entity;

public class Post
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string TextFormat { get; set; } = string.Empty;
    public List<PostFile> Files { get; set; } = [];
    public bool Encrypted { get; set; }
    public PostPending? Pending { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public string? PatreonUpdatedAt { get; set; }
}
