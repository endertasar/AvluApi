namespace AvluApi.Models.Requests;

public class CreateResidentRequest
{
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
}

public class UpdateResidentRequest
{
    public string? FullName { get; set; }
    public string? OneSignalUserId { get; set; }
}
