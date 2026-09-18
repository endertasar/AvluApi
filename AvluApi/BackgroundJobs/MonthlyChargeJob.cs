using System.Text.Json;
using Dapper;
using AvluApi.Infrastructure.Database;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.BackgroundJobs;

public class MonthlyChargeJob : IHostedService, IDisposable
{
    private readonly IServiceProvider        _services;
    private readonly ILogger<MonthlyChargeJob> _logger;
    private Timer? _timer;

    public MonthlyChargeJob(IServiceProvider services, ILogger<MonthlyChargeJob> logger)
    {
        _services = services;
        _logger   = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var now     = DateTime.UtcNow;
        var nextRun = new DateTime(now.Year, now.Month, 1, 6, 0, 0, DateTimeKind.Utc)
            .AddMonths(now.Day == 1 && now.Hour < 6 ? 0 : 1);
        var delay = nextRun - now;

        _logger.LogInformation("MonthlyChargeJob başlatıldı. İlk çalışma: {NextRun}", nextRun);
        _timer = new Timer(ExecuteJob, null, delay, TimeSpan.FromDays(30));
        return Task.CompletedTask;
    }

    private void ExecuteJob(object? state) => _ = ExecuteJobAsync();

    private async Task ExecuteJobAsync()
    {
        var now    = DateTime.UtcNow;
        var period = now.Year * 100 + now.Month;
        _logger.LogInformation("MonthlyChargeJob başladı — Dönem: {Period}", period);

        using var scope    = _services.CreateScope();
        var dbFactory      = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        var duesRepo       = scope.ServiceProvider.GetRequiredService<IDuesRepository>();
        var auditRepo      = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

        try
        {
            using var conn = dbFactory.CreateConnection();

            var properties = (await conn.QueryAsync(
                "SELECT Id, SiteId, Type FROM dbo.Properties WHERE IsActive = 1 AND IsDeleted = 0")).ToList();

            int created = 0, skipped = 0, missing = 0;
            var dueDate = new DateTime(now.Year, now.Month,
                Math.Min(15, DateTime.DaysInMonth(now.Year, now.Month)), 0, 0, 0, DateTimeKind.Utc);

            foreach (var prop in properties)
            {
                long   propId   = prop.Id;
                long   siteId   = prop.SiteId;
                string propType = prop.Type;

                var def = await duesRepo.GetActiveForTypeAndPeriodAsync(siteId, propType, period);
                if (def is null)
                {
                    _logger.LogWarning(
                        "Aidat tanımı bulunamadı — SiteId: {SiteId}, PropertyId: {PropId}, Tip: {Type}, Dönem: {Period}",
                        siteId, propId, propType, period);

                    await auditRepo.WriteAsync(
                        action:   "DuesAccrual",
                        entity:   "MissingDefinition",
                        siteId:   siteId,
                        entityId: propId,
                        detail:   JsonSerializer.Serialize(new { PropertyId = propId, Type = propType, Period = period }));

                    missing++;
                    continue;
                }

                try
                {
                    await conn.ExecuteAsync(
                        """
                        INSERT INTO dbo.DuesCharges (SiteId, PropertyId, Period, Amount, DueDate)
                        VALUES (@SiteId, @PropertyId, @Period, @Amount, @DueDate)
                        """,
                        new { SiteId = siteId, PropertyId = propId, Period = period, def.Amount, DueDate = dueDate });
                    created++;
                }
                catch (Exception ex) when (ex.Message.Contains("UQ_DuesCharges") || ex.Message.Contains("UNIQUE"))
                {
                    skipped++;
                }
            }

            _logger.LogInformation(
                "MonthlyChargeJob tamamlandı — Dönem: {Period}, Oluşturulan: {Created}, Atlanan: {Skipped}, Tanımsız: {Missing}",
                period, created, skipped, missing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MonthlyChargeJob hata — Dönem: {Period}", period);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
