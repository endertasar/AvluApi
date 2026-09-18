using System.Text.Json;
using AvluApi.Infrastructure;

namespace AvluApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlenmeyen hata: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        var (statusCode, title) = exception switch
        {
            UnauthorizedException  => (401, "Yetkisiz"),
            ForbiddenException     => (403, "Yasak"),
            NotFoundException      => (404, "Bulunamadı"),
            ConflictException      => (409, "Çakışma"),
            DomainException        => (400, "İş Kuralı Hatası"),
            _                      => (500, "Sunucu Hatası")
        };

        context.Response.StatusCode = statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail = exception.Message,
            instance = context.Request.Path.ToString()
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
