using System.Security.Cryptography;
using System.Text;

namespace Rasad.Infrastructure.Security;

public static class HmacSignatureService
{
    public static string ComputeSignature(string secretBase64, string payload)
    {
        var key = Convert.FromBase64String(secretBase64);
        using var hmac = new HMACSHA256(key);
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(bytes);
    }
}
