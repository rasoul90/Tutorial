namespace Rasad.Infrastructure.Security;

public class EncryptionOptions
{
    public const string SectionName = "Security";

    public string EncryptionKey { get; set; } = string.Empty;
    public string HashKey { get; set; } = string.Empty;
}
