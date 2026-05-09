namespace Papoa.Entity;

public class PostCreateResult
{
    public Post Post { get; set; } = new();
    public List<PostUploadSession> UploadUrls { get; set; } = [];
}
