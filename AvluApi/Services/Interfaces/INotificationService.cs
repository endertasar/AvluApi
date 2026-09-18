using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;

namespace AvluApi.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetAllAsync(CancellationToken ct = default);
    Task<NotificationDto>              GetByIdAsync(long id, CancellationToken ct = default);
    Task<NotificationDto>              SendAsync(SendNotificationRequest request, CancellationToken ct = default);
}
