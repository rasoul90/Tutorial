using UltraAudit.Application.DTOs;
using Xunit;

namespace UltraAudit.Tests;

/// <summary>
/// اختبارات بسيطة للتأكد من صحة بيانات الأوامر.
/// </summary>
public sealed class CommandCatalogTests
{
    [Fact]
    public void CommandCatalogDto_Should_Have_Defaults()
    {
        var dto = new CommandCatalogDto();
        Assert.NotNull(dto.Name);
        Assert.NotNull(dto.Category);
        Assert.NotNull(dto.Description);
    }
}
