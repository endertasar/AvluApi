namespace AvluApi.Models.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public string UserType { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
}
