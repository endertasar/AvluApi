namespace AvluApi.Models.Requests;

public class SendNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TargetType { get; set; } = "All";
    public long? TargetId { get; set; }
}
