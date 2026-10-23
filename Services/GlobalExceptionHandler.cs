using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SubscriptionPlatform.Api.Services;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger):IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context,Exception exception,CancellationToken cancellationToken){logger.LogError(exception,"Unhandled request exception for {Method} {Path}; trace {TraceId}",context.Request.Method,context.Request.Path,context.TraceIdentifier);context.Response.StatusCode=StatusCodes.Status500InternalServerError;await Results.Problem(statusCode:500,detail:"The request could not be completed.",extensions:new Dictionary<string,object?>{{"traceId",context.TraceIdentifier}}).ExecuteAsync(context);return true;}
}
