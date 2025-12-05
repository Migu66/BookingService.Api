using System.Net;
using System.Text.Json;

namespace BookingService.Api.Infrastructure.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
     _logger.LogError(ex, "An unhandled exception occurred");
  await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse
    {
 Message = "An error occurred while processing your request"
        };

        switch (exception)
        {
  case UnauthorizedAccessException:
     context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Unauthorized access";
             break;
   case ArgumentException:
          case InvalidOperationException:
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
 response.Message = exception.Message;
      break;
case KeyNotFoundException:
      context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        response.Message = exception.Message;
    break;
   default:
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        break;
     }

var result = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}
