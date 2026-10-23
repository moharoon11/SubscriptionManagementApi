using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionPlatform.Api.Models;
using SubscriptionPlatform.Api.Services;

namespace SubscriptionPlatform.Api.Controllers;

[Route("api/v1/admin/applications")]
[Authorize(Policy = "PlatformAdmin")]
public sealed class ApplicationsController(IdentityService identity) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateApplicationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 120)
            return BadRequest(new { error = "invalid_name" });

        var name = request.Name.Trim();
        var result = await identity.RegisterApplicationAsync(name);
        return Created($"api/v1/admin/applications/{result.Id}", new
        {
            id = result.Id,
            name,
            application_key = result.Key
        });
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var applications = await identity.ListApplicationsAsync();
        return Ok(applications);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var deactivated = await identity.DeactivateApplicationAsync(id);
        return deactivated ? NoContent() : NotFound();
    }
}
