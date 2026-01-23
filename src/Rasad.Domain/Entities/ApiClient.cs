namespace Rasad.Domain.Entities;

public class ApiClient
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public byte[] SecretHash { get; set; } = Array.Empty<byte>();
    public bool IsActive { get; set; }
    public Guid? MinistryId { get; set; }
    public Ministry? Ministry { get; set; }
    public string? AllowedIps { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ApiNonce> Nonces { get; set; } = new List<ApiNonce>();
}
