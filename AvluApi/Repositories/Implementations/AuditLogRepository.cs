using Dapper;
using AvluApi.Infrastructure.Database;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbConnectionFactory _factory;

    public AuditLogRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task WriteAsync(
        string action,
        string entity,
        long? siteId = null,
        long? entityId = null,
        string? userType = null,
        long? userId = null,
        string? detail = null,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.AuditLogs (SiteId, UserType, UserId, Action, Entity, EntityId, Detail)
            VALUES (@SiteId, @UserType, @UserId, @Action, @Entity, @EntityId, @Detail);
            """;

        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(sql, new { SiteId = siteId, UserType = userType, UserId = userId, Action = action, Entity = entity, EntityId = entityId, Detail = detail });
    }
}
