// FILE: Helpers/ExceptionHandlingMiddleware.cs
// PURPOSE: The one place an ApiException (see ApiException.cs) becomes an
//          actual HTTP response, so every Controller action can stay a thin
//          "call the service, return what it gives back" method with no
//          repeated try/catch boilerplate - matching
//          04-BACKEND-ARCHITECTURE.md section 3's rule that a controller
//          method doing more than ~10 lines means logic has leaked in.
// LOGIC:   - A thrown ApiException maps straight to its StatusCode + a
//            { "message": ... } body.
//          - Anything else unexpected is logged with full detail server-side
//            but only ever returns a generic 500 message to the client -
//            never leaks a stack trace or internal exception detail over
//            the wire.

using System.Text.Json;

namespace ScanEat.Api.Helpers;

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
        catch (ApiException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "An unexpected error occurred. Please try again." }));
        }
    }
}
