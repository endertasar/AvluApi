namespace AvluApi.Models.DTOs;

public class PropertyDto
{
    public long Id { get; set; }
    public string? Block { get; set; }
    public string UnitNo { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? OwnerPhone { get; set; }
    public string? ResidentName { get; set; }
    public string? ResidentPhone { get; set; }
    public string DuesResponsible { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
