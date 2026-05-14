namespace Papoa.Entity;

public class PostUpdateRequest
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ContentFormat { get; set; }
    public bool IsPublic { get; set; }
    public List<string>? TierNames { get; set; }
    public List<string>? CollectionNames { get; set; }
    public string? PublishDateUtc { get; set; }
    public List<string>? Tags { get; set; }
    public string? PatreonPostId { get; set; }
    public List<PostFile>? AddFiles { get; set; }
    public List<PostFile>? RemoveFiles { get; set; }
    public List<string>? ImageVideoAudioFileNames { get; set; }
    public List<string>? AttachmentFileNames { get; set; }
}
