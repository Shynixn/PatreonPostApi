namespace Papoa.Entity;

public class PostUploadSession
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string> Fields { get; set; } = new();
}
