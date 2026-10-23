using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

namespace SubscriptionPlatform.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("health")]
public sealed class HealthController(HealthCheckService healthChecks, ILogger<HealthController> logger) : ControllerBase
{
    [HttpGet("live")]
    public IActionResult Live() => Ok(new { status = "healthy" });

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        if (report.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy)
            return Ok(new { status = "ready" });

        logger.LogWarning("Readiness check failed: {HealthReport}", string.Join(", ", report.Entries.Select(entry => $"{entry.Key}: {entry.Value.Status}")));
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "not_ready" });
    }
}
