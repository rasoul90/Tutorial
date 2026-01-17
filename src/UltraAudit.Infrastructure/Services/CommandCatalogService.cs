using Microsoft.EntityFrameworkCore;
using UltraAudit.Application.DTOs;
using UltraAudit.Application.Interfaces;
using UltraAudit.Infrastructure.Data;

namespace UltraAudit.Infrastructure.Services;

/// <summary>
/// تنفيذ خدمة كتالوج الأوامر.
/// </summary>
public sealed class CommandCatalogService : ICommandCatalogService
{
    private readonly UasDbContext _dbContext;

    public CommandCatalogService(UasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CommandCatalogDto>> GetCatalogAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.CommandCatalog
            .AsNoTracking()
            .OrderBy(command => command.Category)
            .ThenBy(command => command.Name)
            .Select(command => new CommandCatalogDto
            {
                Id = command.Id,
                Name = command.Name,
                Category = command.Category,
                Description = command.Description,
                ParameterSchemaJson = command.ParameterSchemaJson,
            })
            .ToListAsync(cancellationToken);
    }
}
