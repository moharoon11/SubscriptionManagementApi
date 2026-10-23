using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionPlatform.Api.Services;

namespace SubscriptionPlatform.Api.Controllers;

[Route("api/v1/webhooks/razorpay")]
[AllowAnonymous]
public sealed class RazorpayWebhookController(PaymentService payments) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var eventId = Request.Headers["X-Razorpay-Event-Id"].ToString();
        var signature = Request.Headers["X-Razorpay-Signature"].ToString();

        if (string.IsNullOrWhiteSpace(eventId))
            return BadRequest(new { error = "missing_event_id" });

        var result = await payments.ProcessWebhookAsync(eventId, body, signature);
        if (result == WebhookProcessingResult.InvalidSignature)
            return Unauthorized(new { error = "invalid_signature" });

        return Ok(new
        {
            accepted = true,
            duplicate = result == WebhookProcessingResult.Duplicate
        });
    }
}
