namespace AvluApi.Services.Interfaces;

public interface IOneSignalService
{
    Task SendToPlayerIdsAsync(IEnumerable<string> playerIds, string title, string body, CancellationToken ct = default);
}
