using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    /// <summary>Bildirim geçmişi — Manager/SuperAdmin</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetAll(CancellationToken ct)
    {
        var notifs = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notifs));
    }

    /// <summary>Bildirim detayı — Manager/SuperAdmin</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> GetById(long id, CancellationToken ct)
    {
        var notif = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<NotificationDto>.Ok(notif));
    }

    /// <summary>Bildirim gönder (TargetType: All | Property | Resident) — Manager/SuperAdmin</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> Send(
        [FromBody] SendNotificationRequest request, CancellationToken ct)
    {
        var notif = await _service.SendAsync(request, ct);
        return Ok(ApiResponse<NotificationDto>.Ok(notif, "Bildirim gönderildi."));
    }
}
