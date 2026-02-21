using System.Security.Cryptography;
using System.Text;
using DeliverySaaS.Application.Common.Interfaces;

namespace DeliverySaaS.Infrastructure.Security;

public class HmacSignatureService : IHmacSignatureService
{
    public string ComputeSignature(string payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool ValidateSignature(string payload, string secret, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature)) return false;
        var expected = ComputeSignature(payload, secret);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.Trim().ToLowerInvariant()));
    }
}
