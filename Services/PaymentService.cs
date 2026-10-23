using System.Text.Json;
using SubscriptionPlatform.Api.Repositories;

namespace SubscriptionPlatform.Api.Services;

public enum WebhookProcessingResult { InvalidSignature, Processed, Duplicate }

public sealed class PaymentService(PaymentRepository repository,RazorpayService razorpay,ILogger<PaymentService> logger)
{
    public async Task<bool> VerifyAsync(Guid appId,Guid paymentId,string orderId,string providerPaymentId,string signature){if(!razorpay.VerifyPayment(orderId,providerPaymentId,signature))return false;try{return await repository.VerifyAndActivateAsync(appId,paymentId,orderId,providerPaymentId);}catch(Exception ex){logger.LogError(ex,"Payment verification processing failed for {PaymentId}",paymentId);throw;}}
    public Task<IEnumerable<object>> HistoryAsync(Guid appId)=>repository.HistoryAsync(appId);
    public async Task<WebhookProcessingResult> ProcessWebhookAsync(string eventId,string raw,string signature){if(!razorpay.VerifyWebhook(raw,signature))return WebhookProcessingResult.InvalidSignature;using var doc=JsonDocument.Parse(raw);var root=doc.RootElement;var type=root.GetProperty("event").GetString()??"unknown";string? order=null,payment=null;if(type is "payment.captured" or "payment.failed"){var entity=root.GetProperty("payload").GetProperty("payment").GetProperty("entity");order=entity.GetProperty("order_id").GetString();payment=entity.GetProperty("id").GetString();}try{return await repository.ProcessWebhookAsync(eventId,type,order,payment)?WebhookProcessingResult.Processed:WebhookProcessingResult.Duplicate;}catch(Exception ex){logger.LogError(ex,"Razorpay webhook processing failed for event {EventId}",eventId);throw;}}
}
