using System.Net;
using AiModoo.Core.Exceptions;

namespace AiModoo.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            if (IsAjaxRequest(context))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            else
            {
                context.Response.Redirect("/Home/Error?code=404");
            }
        }
        catch (Core.Exceptions.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            if (IsAjaxRequest(context))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { errors = ex.Errors });
            }
            else
            {
                context.Response.Redirect("/Home/Error?code=400");
            }
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation: {Code}", ex.Code);
            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

            if (IsAjaxRequest(context))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = ex.Message, code = ex.Code });
            }
            else
            {
                context.Response.Redirect("/Home/Error?code=422");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            if (IsAjaxRequest(context))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
            }
            else
            {
                context.Response.Redirect("/Home/Error?code=500");
            }
        }
    }

    private static bool IsAjaxRequest(HttpContext context)
    {
        return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || context.Request.Headers.Accept.ToString().Contains("application/json");
    }
}
