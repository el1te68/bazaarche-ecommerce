using Bazaarche.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Api.Infrastructure;

/// <summary>Converts exceptions into a consistent JSON error: { "message": "..." }.</summary>
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteAsync(context, ex.StatusCode, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await WriteAsync(context, StatusCodes.Status409Conflict, "اطلاعات همزمان توسط کاربر دیگری تغییر کرد. دوباره تلاش کنید.");
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Database update failed");
            await WriteAsync(context, StatusCodes.Status409Conflict, "ثبت اطلاعات به دلیل تداخل با داده‌های موجود انجام نشد.");
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // client disconnected
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string message)
    {
        if (context.Response.HasStarted) return;
        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { message });
    }
}
