using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using UltraAudit.Infrastructure.Data;

namespace UltraAudit.Infrastructure.Migrations;

/// <summary>
/// لقطة نموذج قاعدة البيانات الحالية.
/// </summary>
[DbContext(typeof(UasDbContext))]
partial class UasDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
    }
}
