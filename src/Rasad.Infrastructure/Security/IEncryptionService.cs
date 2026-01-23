namespace Rasad.Infrastructure.Security;

public interface IEncryptionService
{
    byte[] Encrypt(string plainText);
    string Decrypt(byte[] cipherData);
    byte[] ComputeHash(string plainText);
}
