namespace Papoa.Entity;

public class PostPending
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentFormat { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public List<string> TierNames { get; set; } = [];
    public List<string> CollectionNames { get; set; } = [];
    public string? PublishDateUtc { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<PostFile> AddFiles { get; set; } = [];
    public List<PostFile> RemoveFiles { get; set; } = [];
}
