namespace Papoa.Entity;

public class PostPending
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string TextFormat { get; set; } = string.Empty;
    public List<PostFile> AddFiles { get; set; } = [];
    public List<PostFile> RemoveFiles { get; set; } = [];
}
