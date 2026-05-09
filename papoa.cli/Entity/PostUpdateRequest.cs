namespace Papoa.Entity;

public class PostUpdateRequest
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string TextFormat { get; set; } = "text/plain";
    public List<PostFile>? AddFiles { get; set; }
    public List<PostFile>? RemoveFiles { get; set; }
}
