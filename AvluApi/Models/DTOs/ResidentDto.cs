namespace AvluApi.Models.DTOs;

public class ResidentDto
{
    public long Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
