using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionPlatform.Api.Models;
using SubscriptionPlatform.Api.Services;

namespace SubscriptionPlatform.Api.Controllers;

[Route("api/v1/payments")]
[Authorize(Policy = "Application")]
public sealed class PaymentsController(PaymentService payments) : ApiController
{
    [HttpPost("failure")]
    public async Task<IActionResult> Failure(FailedPaymentRequest request)
    {
        var recorded = await payments.MarkFailedAsync(AppId, request.PaymentId, request.OrderId, request.ExternalCustomerId);
        return recorded ? Ok(new { status = "failed" }) : Conflict(new { error = "payment_not_pending" });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(VerifyPaymentRequest request)
    {
        var verified = await payments.VerifyAsync(
            AppId,
            request.PaymentId,
            request.OrderId,
            request.RazorpayPaymentId,
            request.Signature);

        if (!verified)
            return Unauthorized(new { error = "payment_verification_failed" });

        return Ok(new { status = "paid", paymentId = request.PaymentId });
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var paymentsForApplication = await payments.HistoryAsync(AppId);
        return Ok(paymentsForApplication);
    }
}
