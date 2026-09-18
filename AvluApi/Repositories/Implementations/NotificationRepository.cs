using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class NotificationRepository : BaseRepository, INotificationRepository
{
    public NotificationRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<Notification>(new CommandDefinition(
            "SELECT * FROM dbo.Notifications WHERE SiteId = @SiteId AND IsDeleted = 0 ORDER BY SentAt DESC",
            new { SiteId }, cancellationToken: ct));
    }

    public async Task<Notification?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Notification>(new CommandDefinition(
            "SELECT * FROM dbo.Notifications WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { Id = id, SiteId }, cancellationToken: ct));
    }

    public async Task<long> CreateAsync(Notification notification, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(new CommandDefinition(
            """
            INSERT INTO dbo.Notifications (SiteId, Title, Body, TargetType, TargetId, SentByUserId)
            OUTPUT INSERTED.Id
            VALUES (@SiteId, @Title, @Body, @TargetType, @TargetId, @SentByUserId)
            """,
            new { SiteId, notification.Title, notification.Body,
                  notification.TargetType, notification.TargetId, notification.SentByUserId },
            cancellationToken: ct));
    }

    // Tüm site sakinlerinin OneSignal player ID'leri (property üzerinden telefon eşleşmesi)
    public async Task<IEnumerable<string>> GetPlayerIdsForSiteAsync(CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<string>(new CommandDefinition(
            """
            SELECT DISTINCT r.OneSignalUserId
            FROM dbo.Properties p
            JOIN dbo.Residents r ON r.Phone = p.OwnerPhone OR r.Phone = p.ResidentPhone
            WHERE p.SiteId = @SiteId AND p.IsActive = 1 AND p.IsDeleted = 0
              AND r.IsDeleted = 0 AND r.OneSignalUserId IS NOT NULL
            """,
            new { SiteId }, cancellationToken: ct));
    }

    // Belirli bir mülkün sakinlerinin OneSignal player ID'leri
    public async Task<IEnumerable<string>> GetPlayerIdsForPropertyAsync(long propertyId, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<string>(new CommandDefinition(
            """
            SELECT DISTINCT r.OneSignalUserId
            FROM dbo.Properties p
            JOIN dbo.Residents r ON r.Phone = p.OwnerPhone OR r.Phone = p.ResidentPhone
            WHERE p.Id = @PropertyId AND p.SiteId = @SiteId AND p.IsDeleted = 0
              AND r.IsDeleted = 0 AND r.OneSignalUserId IS NOT NULL
            """,
            new { PropertyId = propertyId, SiteId }, cancellationToken: ct));
    }

    // Belirli bir sakin için OneSignal player ID
    public async Task<string?> GetPlayerIdForResidentAsync(long residentId, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<string?>(new CommandDefinition(
            "SELECT OneSignalUserId FROM dbo.Residents WHERE Id = @ResidentId AND IsDeleted = 0",
            new { ResidentId = residentId }, cancellationToken: ct));
    }
}
