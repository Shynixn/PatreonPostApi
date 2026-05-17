using Papoa.Entity;

namespace Papoa.Contract;

public interface IPrintingService
{
    string FilesProp(List<PostFile> files);
}
