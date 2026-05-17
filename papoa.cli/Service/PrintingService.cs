using Papoa.Contract;
using Papoa.Entity;

namespace Papoa.Service;

public class PrintingService : IPrintingService
{
    public string FilesProp(List<PostFile> files) =>
        files.Count == 0 ? "[]" : $"[{string.Join(", ", files.Select(f => f.Name))}]";
}
