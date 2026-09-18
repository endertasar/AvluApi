namespace AvluApi.Models.Requests;

public class CreatePropertyRequest
{
    public string? Block { get; set; }
    public string UnitNo { get; set; } = string.Empty;
    public string Type { get; set; } = "Daire";
    public string? OwnerName { get; set; }
    public string? OwnerPhone { get; set; }
    public string? ResidentName { get; set; }
    public string? ResidentPhone { get; set; }
    public string? DuesResponsible { get; set; }
}

public class UpdatePropertyRequest
{
    public string? Block { get; set; }
    public string UnitNo { get; set; } = string.Empty;
    public string Type { get; set; } = "Daire";
    public string? OwnerName { get; set; }
    public string? OwnerPhone { get; set; }
    public string? ResidentName { get; set; }
    public string? ResidentPhone { get; set; }
    public string? DuesResponsible { get; set; }
    public bool IsActive { get; set; } = true;
}
