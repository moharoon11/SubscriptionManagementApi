using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionPlatform.Api.Models;
using SubscriptionPlatform.Api.Services;

namespace SubscriptionPlatform.Api.Controllers;

[Route("api/v1/plans")]
[Authorize(Policy = "Application")]
public sealed class PlansController(CatalogService catalog) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(PlanRequest request)
    {
        try
        {
            var id = await catalog.CreatePlanAsync(AppId, request);
            return Created($"api/v1/plans/{id}", new
            {
                id,
                request.Name,
                request.Currency,
                request.MonthlyAmountMinor,
                request.YearlyAmountMinor,
                request.TrialDays
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_plan", detail = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var plans = await catalog.ListPlansAsync(AppId);
        return Ok(plans);
    }
}
