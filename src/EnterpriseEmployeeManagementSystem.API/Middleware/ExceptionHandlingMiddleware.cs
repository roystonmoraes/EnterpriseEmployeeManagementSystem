using EnterpriseEmployeeManagementSystem.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace EnterpriseEmployeeManagementSystem.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ConflictException ex)
        {
            _logger.LogWarning(
                ex,
                "A business conflict occurred.");

            context.Response.StatusCode =
                (int)HttpStatusCode.Conflict;

            context.Response.ContentType =
                "application/json";

            var response = new
            {
                status = 409,
                message = ex.Message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "Requested resource was not found.");

            context.Response.StatusCode =
                (int)HttpStatusCode.NotFound;

            context.Response.ContentType =
                "application/json";

            var response = new
            {
                status = 404,
                message = ex.Message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unhandled exception occurred.");

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType =
                "application/json";

            var response = new
            {
                status = 500,
                message = "An unexpected error occurred."
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}