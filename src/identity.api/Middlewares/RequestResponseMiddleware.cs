using System.Diagnostics;
using System.Text;

namespace identity.api.Middlewares;

public class RequestResponseMiddleware : IMiddleware
{
    private readonly ILogger<RequestResponseMiddleware> _logger;

    public RequestResponseMiddleware(ILogger<RequestResponseMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var request = context.Request;
        _logger.LogInformation("Requested {Method} {Path}{Query}",
            request.Method, request.Path, request.QueryString);
        
        var stopwatch = Stopwatch.StartNew();
        
        await next(context);
        
        stopwatch.Stop();
        
        var response = context.Response;
        _logger.LogInformation("{Method} {Path}{Query} - responded {StatusCode} after {Time} milliseconds", 
            request.Method, request.Path, request.QueryString, response.StatusCode, stopwatch.Elapsed.TotalMilliseconds);
    }
}