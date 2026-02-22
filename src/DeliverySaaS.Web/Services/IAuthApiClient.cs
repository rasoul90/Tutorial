using DeliverySaaS.Web.Models;

namespace DeliverySaaS.Web.Services;

public interface IAuthApiClient
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<BranchPrintSettingDto?> GetBranchPrintSettingsAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveBranchPrintSettingsAsync(BranchPrintSettingDto request, CancellationToken cancellationToken = default);
    Task<byte[]?> GenerateLabelPreviewAsync(GenerateLabelsRequest request, CancellationToken cancellationToken = default);
}
