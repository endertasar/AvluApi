using AvluApi.Auth;
using AvluApi.Infrastructure;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository     _repo;
    private readonly IOneSignalService           _push;
    private readonly ISiteContext                _siteContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repo,
        IOneSignalService push,
        ISiteContext siteContext,
        ILogger<NotificationService> logger)
    {
        _repo        = repo;
        _push        = push;
        _siteContext = siteContext;
        _logger      = logger;
    }

    public async Task<IEnumerable<NotificationDto>> GetAllAsync(CancellationToken ct = default)
    {
        var notifs = await _repo.GetAllAsync(ct);
        return notifs.Select(MapDto);
    }

    public async Task<NotificationDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var notif = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Bildirim bulunamadı.");
        return MapDto(notif);
    }

    public async Task<NotificationDto> SendAsync(SendNotificationRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new DomainException("Bildirim başlığı boş olamaz.");
        if (string.IsNullOrWhiteSpace(request.Body))
            throw new DomainException("Bildirim içeriği boş olamaz.");

        // Resolve OneSignal player IDs based on target
        IEnumerable<string> playerIds = request.TargetType switch
        {
            "All"      => await _repo.GetPlayerIdsForSiteAsync(ct),
            "Property" => await _repo.GetPlayerIdsForPropertyAsync(request.TargetId!.Value, ct),
            "Resident" => ToList(await _repo.GetPlayerIdForResidentAsync(request.TargetId!.Value, ct)),
            _          => []
        };

        if (request.TargetType is "Property" or "Resident" && request.TargetId is null)
            throw new DomainException("Property ve Resident hedefleri için TargetId zorunludur.");

        // Save to DB first (we record intent regardless of push delivery)
        var notification = new Notification
        {
            Title        = request.Title,
            Body         = request.Body,
            TargetType   = request.TargetType,
            TargetId     = request.TargetId,
            SentByUserId = _siteContext.UserId > 0 ? _siteContext.UserId : null
        };
        notification.Id = await _repo.CreateAsync(notification, ct);

        // Fire push — failure is logged inside OneSignalService, not thrown
        await _push.SendToPlayerIdsAsync(playerIds, request.Title, request.Body, ct);

        return MapDto(notification);
    }

    private static IEnumerable<string> ToList(string? id) =>
        id is null ? [] : [id];

    private static NotificationDto MapDto(Notification n) => new()
    {
        Id           = n.Id,
        Title        = n.Title,
        Body         = n.Body,
        TargetType   = n.TargetType,
        TargetId     = n.TargetId,
        SentAt       = n.SentAt,
        SentByUserId = n.SentByUserId
    };
}
