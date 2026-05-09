using Papoa.Contract;
using Papoa.Entity;

namespace Papoa.Service;

public class FileUploadService(HttpClient httpClient) : IFileUploadService
{
    /// <summary>
    /// Uploads a local file to the presigned URL specified in the upload session.
    /// When <paramref name="password"/> is non-null the file is encrypted on-the-fly
    /// with AES-256-CBC — it is never fully loaded into memory.
    /// </summary>
    public async Task UploadFileAsync(PostUploadSession session, string filePath, string? password)
    {
        var fileStream = File.OpenRead(filePath);
        Stream uploadStream = password is not null
            ? new CbcEncryptingStream(fileStream, password)
            : fileStream;

        await using var _ = uploadStream;
        using var content = new MultipartFormDataContent();
        foreach (var field in session.Fields)
        {
            content.Add(new StringContent(field.Value), field.Key);
        }
        content.Add(new StreamContent(uploadStream), "file", Path.GetFileName(filePath));

        var response = await httpClient.PostAsync(session.Url, content);
        response.EnsureSuccessStatusCode();
    }
}
