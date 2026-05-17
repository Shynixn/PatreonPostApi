namespace Papoa.Entity;

public class Post
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentFormat { get; set; } = string.Empty;
    public List<PostFile> Files { get; set; } = [];
    public bool Encrypted { get; set; }
    public bool IsPublic { get; set; }
    public List<string> TierNames { get; set; } = [];
    public List<string> CollectionNames { get; set; } = [];
    public string? PublishDateUtc { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<string> PhotoAttachmentFileNames { get; set; } = [];
    public List<string> AttachmentFileNames { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public string PatreonPostId { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public string FilesExpireAt { get; set; } = string.Empty;
    public string? PatreonUpdatedAt { get; set; }
}
