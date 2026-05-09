using Papoa.Entity;

namespace Papoa.Contract;

public interface IFileUploadService
{
    /// <summary>
    /// Uploads a file to the given upload session URL.
    /// When <paramref name="password"/> is non-null the file is encrypted on-the-fly
    /// with AES-256-CBC before transmission; otherwise it is sent as-is.
    /// </summary>
    public Task UploadFileAsync(PostUploadSession session, string filePath, string? password);
}