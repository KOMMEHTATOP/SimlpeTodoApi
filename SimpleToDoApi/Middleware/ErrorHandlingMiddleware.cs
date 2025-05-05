using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace SimpleToDoApi.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Ошибка обновления данных в БД");
            await HandleExceptionAsync(context, dbEx, HttpStatusCode.BadRequest, "Ошибка обновления данных в базе.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка");
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "Внутренняя ошибка сервера");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode code, string message)
    {
        context.Response.StatusCode = (int)code;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            status = context.Response.StatusCode,
            message = message,
#if DEBUG //директива компилятора. Запускается только в режиме отладки.
            detail = exception.Message
#endif
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}
