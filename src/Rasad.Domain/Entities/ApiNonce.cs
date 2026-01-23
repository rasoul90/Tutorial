namespace Rasad.Domain.Entities;

public class ApiNonce
{
    public Guid Id { get; set; }
    public Guid ApiClientId { get; set; }
    public ApiClient? ApiClient { get; set; }
    public string Nonce { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}
