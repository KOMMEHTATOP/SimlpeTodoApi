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
            _logger.LogError(dbEx, "Database update error during request {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(context, dbEx, HttpStatusCode.BadRequest, "Database update error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing request {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(context, ex, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private async Task WriteProblemDetailsAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode code,
        string title)
    {
        context.Response.StatusCode = (int)code;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            type = code == HttpStatusCode.InternalServerError
                ? "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                : "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = title,
            status = (int)code,
            detail =
#if DEBUG
                exception.Message,
#else
                code == HttpStatusCode.InternalServerError
                    ? "An unexpected error occurred. Please contact the administrator."
                    : exception.Message,
#endif
            instance = context.Request.Path
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }
}