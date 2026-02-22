using DeliverySaaS.Web.Models;

namespace DeliverySaaS.Web.Services;

public interface IAuthApiClient
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
