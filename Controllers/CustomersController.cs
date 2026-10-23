using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionPlatform.Api.Models;
using SubscriptionPlatform.Api.Services;

namespace SubscriptionPlatform.Api.Controllers;

[Route("api/v1/customers")]
[Authorize(Policy = "Application")]
public sealed class CustomersController(CatalogService catalog) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(CustomerRequest request)
    {
        try
        {
            var customer = await catalog.CreateCustomerAsync(AppId, request);
            return Ok(customer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_customer", detail = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var customers = await catalog.ListCustomersAsync(AppId);
        return Ok(customers);
    }
}
