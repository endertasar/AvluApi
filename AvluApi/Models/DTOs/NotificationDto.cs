namespace AvluApi.Models.DTOs;

public class NotificationDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public long? TargetId { get; set; }
    public DateTime SentAt { get; set; }
    public long? SentByUserId { get; set; }
}
