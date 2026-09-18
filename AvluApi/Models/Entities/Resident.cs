namespace AvluApi.Models.Entities;

public class Resident
{
    public long Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool IsPhoneVerified { get; set; } = true;
    public string? OneSignalUserId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
