namespace Papoa.Entity;

public class PostCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? ContentFormat { get; set; }
    public bool? IsPublic { get; set; }
    public List<string>? TierNames { get; set; }
    public List<string>? CollectionNames { get; set; }
    public string? PublishDateUtc { get; set; }
    public List<string>? Tags { get; set; }
    public int? TtlDays { get; set; }
    public bool Encrypted { get; set; }
    public List<PostFile>? Files { get; set; }
    public List<string>? PhotoAttachmentFileNames { get; set; }
    public List<string>? AttachmentFileNames { get; set; }
}
