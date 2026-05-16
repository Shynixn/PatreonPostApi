using Papoa.Entity;

namespace Papoa.Contract;

public interface IFileUploadService
{
    /// <summary>
    /// Encrypts <paramref name="plaintext"/> with AES-256-CBC using the given password.
    /// Wire format: salt(16) | IV(16) | AES-256-CBC ciphertext (PKCS7 padded).
    /// </summary>
    byte[] Encrypt(byte[] plaintext, string password);

    /// <summary>
    /// Uploads <paramref name="fileBytes"/> to the presigned URL specified in the upload session.
    /// The bytes are sent as-is; encryption must be applied by the caller if needed.
    /// </summary>
    Task UploadBytesAsync(PostUploadSession session, byte[] fileBytes, string fileName);

    /// <summary>
    /// Reads <paramref name="filePath"/> from disk and uploads it.
    /// When <paramref name="password"/> is non-null the file is encrypted before upload.
    /// Prefer pre-encrypting with <see cref="Encrypt"/> and calling <see cref="UploadBytesAsync"/>
    /// when the encrypted size must be known in advance.
    /// </summary>
    Task UploadFileAsync(PostUploadSession session, string filePath, string? password);
}