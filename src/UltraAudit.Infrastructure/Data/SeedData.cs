using Microsoft.EntityFrameworkCore;
using UltraAudit.Domain.Entities;

namespace UltraAudit.Infrastructure.Data;

/// <summary>
/// تهيئة البيانات الأولية للتطبيق.
/// </summary>
public static class SeedData
{
    public static async Task EnsureSeedDataAsync(UasDbContext dbContext)
    {
        if (!await dbContext.CommandCatalog.AnyAsync())
        {
            var commands = new List<CommandCatalogItem>
            {
                new() { Name = "Import", Category = "Data Preparation", Description = "استيراد البيانات عبر المعالج.", ParameterSchemaJson = "{\"type\":\"wizard\"}" },
                new() { Name = "Field Type Normalize", Category = "Data Preparation", Description = "توحيد أنواع الحقول.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Trim/Clean Text", Category = "Data Preparation", Description = "تنظيف النصوص من الفراغات.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Date Parse/Normalize", Category = "Data Preparation", Description = "توحيد صيغ التاريخ.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Remove Duplicates", Category = "Data Preparation", Description = "إزالة التكرارات المتطابقة.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Fuzzy Duplicate Candidates", Category = "Data Preparation", Description = "الكشف عن التكرارات التقريبية.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Filter", Category = "Investigation", Description = "تصفية البيانات عبر منشئ الاستعلام.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Summarize", Category = "Investigation", Description = "تلخيص وتجميع البيانات.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Stratify", Category = "Investigation", Description = "تقسيم البيانات إلى طبقات.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Aging", Category = "Investigation", Description = "تحليل الأعمار حسب التاريخ.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Top/Bottom", Category = "Investigation", Description = "أعلى/أدنى السجلات.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Sequence Check", Category = "Investigation", Description = "فحص الفجوات في التسلسل.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Duplicates", Category = "Investigation", Description = "الكشف عن التكرارات بالمفتاح.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Benford", Category = "Investigation", Description = "تحليل بنفورد للرقم الأول.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Join/Relate", Category = "Investigation", Description = "ربط مجموعات البيانات.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Compare", Category = "Investigation", Description = "مقارنة مجموعات البيانات.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Cross-tab", Category = "Investigation", Description = "ملخص محوري.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Random Sample", Category = "Sampling", Description = "عينة عشوائية.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Systematic Sample", Category = "Sampling", Description = "عينة منتظمة.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Stratified Sample", Category = "Sampling", Description = "عينة طبقية.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "MUS", Category = "Sampling", Description = "معاينة وحدات نقدية.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Attribute Sampling", Category = "Sampling", Description = "عينة صفات.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Computed Field", Category = "Calculations", Description = "حقل محسوب.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Running Total", Category = "Calculations", Description = "إجمالي تراكمي.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Percent of Total", Category = "Calculations", Description = "نسبة من الإجمالي.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Date Diff", Category = "Calculations", Description = "فرق بين تاريخين.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Export Results to Excel", Category = "Export", Description = "تصدير النتائج إلى Excel.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Export Results to PDF", Category = "Export", Description = "تصدير النتائج إلى PDF.", ParameterSchemaJson = "{\"type\":\"object\"}" },
                new() { Name = "Save Results as Dataset", Category = "Export", Description = "حفظ النتائج كمجموعة بيانات.", ParameterSchemaJson = "{\"type\":\"object\"}" },
            };
            dbContext.CommandCatalog.AddRange(commands);
        }

        if (!await dbContext.Projects.AnyAsync())
        {
            var project = new Project
            {
                Name = "Payroll Oversight",
                Code = "PAY-2024",
                Description = "تدقيق بيانات الرواتب والبدلات.",
            };
            dbContext.Projects.Add(project);

            var payrollDataset = new Dataset
            {
                ProjectId = project.Id,
                Name = "Payroll Dataset",
                SourceType = "CSV",
                StorageTable = "stg_payroll_2024",
                RowCount = 1200,
            };

            var budgetDataset = new Dataset
            {
                ProjectId = project.Id,
                Name = "Budget Tabulation",
                SourceType = "Excel",
                StorageTable = "stg_budget_2024",
                RowCount = 450,
            };

            dbContext.Datasets.AddRange(payrollDataset, budgetDataset);

            dbContext.ExceptionCases.Add(new ExceptionCase
            {
                ProjectId = project.Id,
                AnalysisRunId = Guid.NewGuid(),
                Title = "ازدواجية بدل سكن",
                Status = "New",
                DueDateUtc = DateTime.UtcNow.AddDays(14),
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
