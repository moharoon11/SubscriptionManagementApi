using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionPlatform.Api.Models;
using SubscriptionPlatform.Api.Services;

namespace SubscriptionPlatform.Api.Controllers;

[Route("api/v1/subscriptions")]
[Authorize(Policy = "Application")]
public sealed class SubscriptionsController(SubscriptionService subscriptions) : ApiController
{
    [HttpGet("by-customer/{externalId}")]
    public async Task<IActionResult> GetByCustomer(string externalId) => Ok(await subscriptions.GetByExternalCustomerAsync(AppId, externalId));
    [HttpPost]
    public async Task<IActionResult> Create(CreateSubscriptionRequest request)
    {
        try
        {
            var subscription = await subscriptions.CreateAsync(AppId, request);
            if (subscription is null)
                return NotFound(new { error = "customer_or_plan_not_found" });

            return CreatedAtAction(nameof(Get), new { id = subscription.SubscriptionId }, subscription);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_subscription", detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = "subscription_conflict", detail = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var subscription = await subscriptions.GetAsync(id, AppId);
        return subscription is null ? NotFound() : Ok(subscription);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var cancelled = await subscriptions.CancelAsync(id, AppId);
        return cancelled ? Ok(new { id, cancelAtPeriodEnd = true }) : Conflict();
    }

    [HttpPost("{id:guid}/change-plan")]
    public async Task<IActionResult> ChangePlan(Guid id, [FromQuery] Guid planId)
    {
        var changed = await subscriptions.ChangePlanAsync(id, AppId, planId);
        return changed ? Ok(new { id, planId }) : Conflict();
    }
}
