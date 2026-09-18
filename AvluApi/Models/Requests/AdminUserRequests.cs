namespace AvluApi.Models.Requests;

public class CreateAdminUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = "SubUser";
}

public class UpdateAdminUserRequest
{
    public string? FullName { get; set; }
    public string Role { get; set; } = "SubUser";
    public bool IsActive { get; set; } = true;
}
