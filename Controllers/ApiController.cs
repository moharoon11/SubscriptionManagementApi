using Microsoft.AspNetCore.Mvc;

namespace SubscriptionPlatform.Api.Controllers;

[ApiController]
public abstract class ApiController : ControllerBase
{
    protected Guid AppId => Guid.TryParse(User.FindFirst("app_id")?.Value, out var id) ? id : Guid.Empty;
}
