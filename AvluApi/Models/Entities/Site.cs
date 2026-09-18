namespace AvluApi.Models.Entities;

public class Site
{
    public long Id { get; set; }
    public string SiteUsername { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
