namespace Papoa.Entity;

public class PostUpdateRequest
{
    public string Id { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ContentFormat { get; set; }
    public string? Status { get; set; }
    public bool? IsPublic { get; set; }
    public List<string>? TierNames { get; set; }
    public List<string>? CollectionNames { get; set; }
    public List<string>? Tags { get; set; }
    public string? PatreonPostId { get; set; }
}
