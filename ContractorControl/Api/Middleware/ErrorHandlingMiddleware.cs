using System.Net;
using System.Text.Json;
using ContractorControl.Application.DTOs;

namespace ContractorControl.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var response = new ApiResponse { Status = false, Message = exception.Message, Data = new {} };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
