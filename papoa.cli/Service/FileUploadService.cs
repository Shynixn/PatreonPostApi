using System.Security.Cryptography;
using Papoa.Contract;
using Papoa.Entity;

namespace Papoa.Service;

public class FileUploadService(HttpClient httpClient) : IFileUploadService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Pbkdf2Iterations = 600_000;

    /// <summary>
    /// Uploads a local file to the presigned URL specified in the upload session.
    /// When <paramref name="password"/> is non-null the file is fully loaded into memory,
    /// encrypted with AES-256-CBC, and uploaded as ciphertext.
    /// Wire format: salt(16) | IV(16) | AES-256-CBC ciphertext (PKCS7 padded).
    /// </summary>
    public async Task UploadFileAsync(PostUploadSession session, string filePath, string? password)
    {
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var uploadBytes = password is not null ? Encrypt(fileBytes, password) : fileBytes;

        using var content = new MultipartFormDataContent();
        foreach (var field in session.Fields)
        {
            content.Add(new StringContent(field.Value), field.Key);
        }
        content.Add(new ByteArrayContent(uploadBytes), "file", Path.GetFileName(filePath));

        var response = await httpClient.PostAsync(session.Url, content);
        response.EnsureSuccessStatusCode();
    }

    private static byte[] Encrypt(byte[] plaintext, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, KeySize);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        var ciphertext = aes.EncryptCbc(plaintext, aes.IV);
        return [.. salt, .. aes.IV, .. ciphertext];
    }
}
