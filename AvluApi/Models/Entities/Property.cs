namespace AvluApi.Models.Entities;

public class Property
{
    public long Id { get; set; }
    public long SiteId { get; set; }
    public string? Block { get; set; }
    public string UnitNo { get; set; } = string.Empty;
    public string Type { get; set; } = "Daire";
    public string? OwnerName { get; set; }
    public string? OwnerPhone { get; set; }
    public string? ResidentName { get; set; }
    public string? ResidentPhone { get; set; }
    public string DuesResponsible { get; set; } = "Owner";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
