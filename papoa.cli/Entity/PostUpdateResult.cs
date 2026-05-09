namespace Papoa.Entity;

public class PostUpdateResult
{
    public Post Post { get; set; } = new();
    public List<PostUploadSession> UploadUrls { get; set; } = [];
}
