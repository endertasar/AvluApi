namespace AvluApi.Repositories.Interfaces;

public interface IAuditLogRepository
{
    Task WriteAsync(
        string action,
        string entity,
        long? siteId = null,
        long? entityId = null,
        string? userType = null,
        long? userId = null,
        string? detail = null,
        CancellationToken ct = default);
}
