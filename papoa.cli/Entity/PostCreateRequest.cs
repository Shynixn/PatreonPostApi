namespace Papoa.Entity;

public class PostCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string TextFormat { get; set; } = "text/plain";
    public bool Encrypted { get; set; }
    public List<PostFile>? AddFiles { get; set; }
}
