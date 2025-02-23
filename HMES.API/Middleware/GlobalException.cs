using HMES.Data.DTO.Custom;

namespace HMES.API.Middleware;

public class GlobalExceptionMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                CustomException e when e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status404NotFound,
                CustomException e when e.Message.Contains("denied", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status403Forbidden,
                CustomException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };


            var reponse = new
            {
                StatusCodes = context.Response.StatusCode,
                ex.Message,
                Detailed = context.Response.StatusCode == StatusCodes.Status500InternalServerError ? ex.ToString() : null
            };
            await context.Response.WriteAsJsonAsync(reponse);
        }
    }
}