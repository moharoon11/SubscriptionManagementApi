using Microsoft.AspNetCore.Mvc;
using SubscriptionPlatform.Api.Models;
using SubscriptionPlatform.Api.Services;

namespace SubscriptionPlatform.Api.Controllers;

[Route("api/v1/auth")]
public sealed class AuthController(IdentityService identity) : ApiController
{
    [HttpPost("token")]
    public async Task<IActionResult> CreateToken(LoginRequest request)
    {
        var token = await identity.IssueTokenAsync(request);
        if (token is null)
            return Unauthorized();

        return Ok(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = 3600
        });
    }
}
