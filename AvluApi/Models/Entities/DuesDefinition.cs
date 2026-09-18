namespace AvluApi.Models.Entities;

public class DuesDefinition
{
    public long Id { get; set; }
    public long SiteId { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int EffectiveFrom { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
