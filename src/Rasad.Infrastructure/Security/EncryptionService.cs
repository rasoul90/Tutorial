using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Rasad.Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _encryptionKey;
    private readonly byte[] _hashKey;

    public EncryptionService(IOptions<EncryptionOptions> options)
    {
        _encryptionKey = Convert.FromBase64String(options.Value.EncryptionKey);
        _hashKey = Convert.FromBase64String(options.Value.HashKey);
    }

    public byte[] Encrypt(string plainText)
    {
        var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipherBytes = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Encrypt(nonce, plaintextBytes, cipherBytes, tag);

        var output = new byte[nonce.Length + cipherBytes.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, nonce.Length);
        Buffer.BlockCopy(cipherBytes, 0, output, nonce.Length, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, output, nonce.Length + cipherBytes.Length, tag.Length);
        return output;
    }

    public string Decrypt(byte[] cipherData)
    {
        var nonce = new byte[NonceSize];
        Buffer.BlockCopy(cipherData, 0, nonce, 0, nonce.Length);

        var cipherLength = cipherData.Length - NonceSize - TagSize;
        var cipherBytes = new byte[cipherLength];
        Buffer.BlockCopy(cipherData, nonce.Length, cipherBytes, 0, cipherLength);

        var tag = new byte[TagSize];
        Buffer.BlockCopy(cipherData, nonce.Length + cipherLength, tag, 0, tag.Length);

        var plaintextBytes = new byte[cipherLength];
        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }

    public byte[] ComputeHash(string plainText)
    {
        using var hmac = new HMACSHA256(_hashKey);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(plainText));
    }
}
