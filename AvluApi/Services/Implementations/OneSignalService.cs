using System.Net.Http.Json;
using System.Text.Json;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class OneSignalService : IOneSignalService
{
    private readonly HttpClient                _http;
    private readonly string                    _appId;
    private readonly ILogger<OneSignalService> _logger;

    public OneSignalService(IHttpClientFactory factory, IConfiguration configuration, ILogger<OneSignalService> logger)
    {
        _http   = factory.CreateClient("OneSignal");
        _appId  = configuration["OneSignal:AppId"] ?? string.Empty;
        _logger = logger;
    }

    public async Task SendToPlayerIdsAsync(
        IEnumerable<string> playerIds, string title, string body, CancellationToken ct = default)
    {
        var ids = playerIds.ToList();
        if (ids.Count == 0)
        {
            _logger.LogWarning("OneSignal push atlandı — kayıtlı player ID yok.");
            return;
        }

        var payload = new
        {
            app_id          = _appId,
            include_player_ids = ids,
            headings        = new { en = title },
            contents        = new { en = body }
        };

        try
        {
            using var response = await _http.PostAsJsonAsync(
                "/api/v1/notifications", payload, ct);

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("OneSignal hata döndü — Status: {Status}, Body: {Body}",
                    (int)response.StatusCode, raw);
            }
            else
            {
                _logger.LogInformation("OneSignal push gönderildi — {Count} cihaz, Başlık: {Title}",
                    ids.Count, title);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OneSignal push isteği başarısız — Başlık: {Title}", title);
        }
    }
}
