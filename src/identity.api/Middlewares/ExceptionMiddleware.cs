namespace identity.api.Middlewares;

public class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger) =>
        _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            await HandleException(context, e);
        }
    }

    private Task HandleException(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = 500;
        const string errorMessage = "An error occurred during processing this request";
        _logger.LogError(exception, errorMessage);
        return context.Response.WriteAsync(errorMessage);
    }
}