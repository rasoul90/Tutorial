using Rasad.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace Rasad.Web.Models.Reports;

public class ReportFilterViewModel
{
    [Display(ResourceType = typeof(SharedResources), Name = "MinistryLabel")]
    public Guid? MinistryId { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "DirectorateLabel")]
    public Guid? DirectorateId { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "DepartmentLabel")]
    public Guid? DepartmentId { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "SectionLabel")]
    public Guid? SectionId { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "UnitLabel")]
    public Guid? UnitId { get; set; }
}
