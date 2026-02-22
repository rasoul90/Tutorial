using System.Net.Http.Json;
using DeliverySaaS.Web.Models;

namespace DeliverySaaS.Web.Services;

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
    }

    public async Task<BranchPrintSettingDto?> GetBranchPrintSettingsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/api/branch/print/settings", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<BranchPrintSettingDto>(cancellationToken: cancellationToken);
    }

    public async Task<bool> SaveBranchPrintSettingsAsync(BranchPrintSettingDto request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync("/api/branch/print/settings", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<byte[]?> GenerateLabelPreviewAsync(GenerateLabelsRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/branch/print/labels", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
