namespace Rasad.Domain.Entities;

public class Unit
{
    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public Section? Section { get; set; }
    public string NameAr { get; set; } = string.Empty;
}
