using Papoa.Entity;

namespace Papoa.Contract;

public interface IPrintingService
{
    string FilesProp(List<PostFile> files);
    void PrintMessage(string message, string outputFormat);
    void PrintPost(Post post, string outputFormat);
    void PrintPosts(List<Post> posts, string outputFormat);
}
