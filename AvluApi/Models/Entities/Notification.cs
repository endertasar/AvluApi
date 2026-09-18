namespace AvluApi.Models.Entities;

public class Notification
{
    public long Id { get; set; }
    public long SiteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TargetType { get; set; } = "All";
    public long? TargetId { get; set; }
    public long? SentByUserId { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsDeleted { get; set; }
}
