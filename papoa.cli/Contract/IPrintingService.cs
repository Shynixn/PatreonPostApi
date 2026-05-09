using Papoa.Entity;

namespace Papoa.Contract;

public interface IPrintingService
{
    string StringProp(string current, string? pending);
    string FilesProp(List<PostFile> current, List<PostFile>? add, List<PostFile>? remove);
}
