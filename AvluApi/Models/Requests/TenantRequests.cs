namespace AvluApi.Models.Requests;

public class CreateTenantRequest
{
    public string TenantName { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    public string? Address { get; set; }
}

public class UpdateTenantRequest
{
    public string TenantName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AssignAdminRequest
{
    public long AdminId { get; set; }
    public string Role { get; set; } = "Manager";
}
