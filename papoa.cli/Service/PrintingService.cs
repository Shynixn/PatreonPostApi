using Papoa.Contract;
using Papoa.Entity;

namespace Papoa.Service;

public class PrintingService : IPrintingService
{
    public string StringProp(string current, string? pending) =>
        pending != null && pending != current
            ? $"\"{current}\" -> \"{pending}\""
            : current;

    public string FilesProp(List<PostFile> current, List<PostFile>? add, List<PostFile>? remove)
    {
        var currentStr = FormatFileList(current);
        if (add == null && remove == null)
            return currentStr;

        var pendingFiles = current
            .Where(f => remove == null || !remove.Any(r => r.Name == f.Name))
            .Concat(add ?? [])
            .ToList();
        var pendingStr = FormatFileList(pendingFiles);
        return currentStr == pendingStr ? currentStr : $"{currentStr} -> {pendingStr}";
    }

    private string FormatFileList(List<PostFile> files) =>
        files.Count == 0 ? "[]" : $"[{string.Join(", ", files.Select(f => f.Name))}]";
}
