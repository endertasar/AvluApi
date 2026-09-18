using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<long> CreateAsync(Notification notification, CancellationToken ct = default);
    Task<IEnumerable<string>> GetPlayerIdsForSiteAsync(CancellationToken ct = default);
    Task<IEnumerable<string>> GetPlayerIdsForPropertyAsync(long propertyId, CancellationToken ct = default);
    Task<string?> GetPlayerIdForResidentAsync(long residentId, CancellationToken ct = default);
}
