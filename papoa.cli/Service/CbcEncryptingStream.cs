using System.Security.Cryptography;

namespace Papoa.Service;

/// <summary>
/// A read-only stream that prepends a 32-byte header (16-byte salt + 16-byte IV)
/// followed by the AES-256-CBC ciphertext encrypted on-the-fly.
/// Wire format: salt(16) | IV(16) | AES-256-CBC ciphertext (PKCS7 padded).
/// </summary>
internal sealed class CbcEncryptingStream : Stream
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Pbkdf2Iterations = 600_000;

    private readonly Aes _aes;
    private readonly CryptoStream _cryptoStream;
    private readonly byte[] _header;
    private int _headerPosition;

    internal CbcEncryptingStream(Stream plaintext, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, KeySize);
        _aes = Aes.Create();
        _aes.Key = key;
        _aes.GenerateIV();
        _aes.Mode = CipherMode.CBC;
        _aes.Padding = PaddingMode.PKCS7;
        _header = [.. salt, .. _aes.IV];
        _cryptoStream = new CryptoStream(plaintext, _aes.CreateEncryptor(), CryptoStreamMode.Read);
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_headerPosition < _header.Length)
        {
            var toCopy = Math.Min(count, _header.Length - _headerPosition);
            Array.Copy(_header, _headerPosition, buffer, offset, toCopy);
            _headerPosition += toCopy;
            return toCopy;
        }

        return _cryptoStream.Read(buffer, offset, count);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_headerPosition < _header.Length)
        {
            var toCopy = Math.Min(count, _header.Length - _headerPosition);
            Array.Copy(_header, _headerPosition, buffer, offset, toCopy);
            _headerPosition += toCopy;
            return toCopy;
        }

        return await _cryptoStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override void Flush() { }
    public override void SetLength(long value) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cryptoStream.Dispose();
            _aes.Dispose();
        }
        base.Dispose(disposing);
    }
}
