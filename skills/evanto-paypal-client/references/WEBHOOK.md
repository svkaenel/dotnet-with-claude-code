# Webhook Handler Reference

Complete implementation for handling PayPal webhook events for both one-time payments and subscriptions.

## Webhook Controller

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/paypal/webhooks")]
public class PayPalWebhookController : ControllerBase
{
    private readonly ILogger<PayPalWebhookController> mLogger;

    public PayPalWebhookController(ILogger<PayPalWebhookController> logger)
    {
        mLogger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        // TODO: Verify webhook signature using PayPal-provided headers
        // var transmissionId   = Request.Headers["PayPal-Transmission-Id"].FirstOrDefault();
        // var transmissionTime = Request.Headers["PayPal-Transmission-Time"].FirstOrDefault();
        // var certUrl          = Request.Headers["PayPal-Cert-Url"].FirstOrDefault();
        // var authAlgo         = Request.Headers["PayPal-Auth-Algo"].FirstOrDefault();
        // var transmissionSig  = Request.Headers["PayPal-Transmission-Sig"].FirstOrDefault();

        JsonDocument webhookEvent;
        try
        {
            webhookEvent = JsonDocument.Parse(body);
        }
        catch (JsonException ex)
        {
            mLogger.LogWarning(ex, "Invalid webhook JSON payload");
            return BadRequest("Invalid JSON");
        }

        var eventType = webhookEvent.RootElement
            .GetProperty("event_type")
            .GetString();

        mLogger.LogInformation("Received PayPal webhook: {EventType}", eventType);

        switch (eventType)
        {
            // Subscription lifecycle events
            case "BILLING.SUBSCRIPTION.ACTIVATED":
                await HandleSubscriptionActivated(webhookEvent);
                break;

            case "BILLING.SUBSCRIPTION.UPDATED":
                await HandleSubscriptionUpdated(webhookEvent);
                break;

            case "BILLING.SUBSCRIPTION.SUSPENDED":
                await HandleSubscriptionSuspended(webhookEvent);
                break;

            case "BILLING.SUBSCRIPTION.CANCELLED":
                await HandleSubscriptionCancelled(webhookEvent);
                break;

            case "BILLING.SUBSCRIPTION.EXPIRED":
                await HandleSubscriptionExpired(webhookEvent);
                break;

            // Payment events
            case "PAYMENT.SALE.COMPLETED":
                await HandlePaymentCompleted(webhookEvent);
                break;

            case "PAYMENT.SALE.DENIED":
                await HandlePaymentDenied(webhookEvent);
                break;

            case "PAYMENT.SALE.REFUNDED":
                await HandlePaymentRefunded(webhookEvent);
                break;

            default:
                mLogger.LogInformation("Unhandled webhook event type: {EventType}", eventType);
                break;
        }

        // Always return 200 to acknowledge receipt
        return Ok();
    }

    private async Task HandleSubscriptionActivated(JsonDocument webhookEvent)
    {
        var resource = webhookEvent.RootElement.GetProperty("resource");
        var subscriptionId = resource.GetProperty("id").GetString();

        mLogger.LogInformation("Subscription activated: {SubscriptionId}", subscriptionId);

        // 1. Find subscription in your database by PayPal subscription ID
        // 2. Update status to "active"
        // 3. Grant premium access to the user
        // 4. Send confirmation email

        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionUpdated(JsonDocument webhookEvent)
    {
        var resource = webhookEvent.RootElement.GetProperty("resource");
        var subscriptionId = resource.GetProperty("id").GetString();

        mLogger.LogInformation("Subscription updated: {SubscriptionId}", subscriptionId);

        // Sync subscription data from PayPal (next billing date, etc.)

        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionSuspended(JsonDocument webhookEvent)
    {
        var resource = webhookEvent.RootElement.GetProperty("resource");
        var subscriptionId = resource.GetProperty("id").GetString();

        mLogger.LogInformation("Subscription suspended: {SubscriptionId}", subscriptionId);

        // 1. Update status to "suspended"
        // 2. Optionally revoke premium access
        // 3. Send suspension notification email

        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionCancelled(JsonDocument webhookEvent)
    {
        var resource = webhookEvent.RootElement.GetProperty("resource");
        var subscriptionId = resource.GetProperty("id").GetString();

        mLogger.LogInformation("Subscription cancelled: {SubscriptionId}", subscriptionId);

        // 1. Update status to "cancelled", set end date
        // 2. Revoke premium access (or schedule revocation at end of billing period)
        // 3. Send cancellation email

        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionExpired(JsonDocument webhookEvent)
    {
        var resource = webhookEvent.RootElement.GetProperty("resource");
        var subscriptionId = resource.GetProperty("id").GetString();

        mLogger.LogInformation("Subscription expired: {SubscriptionId}", subscriptionId);

        // 1. Update status to "expired"
        // 2. Revoke premium access

        await Task.CompletedTask;
    }

    private async Task HandlePaymentCompleted(JsonDocument webhookEvent)
    {
        var resource = webhookEvent.RootElement.GetProperty("resource");

        // Extract payment details
        var transactionId = resource.GetProperty("id").GetString();
        var amount = resource.GetProperty("amount").GetProperty("total").GetString();
        var currency = resource.GetProperty("amount").GetProperty("currency").GetString();

        // Extract subscription ID from billing_agreement_id (for subscription payments)
        String? subscriptionId = null;
        if (resource.TryGetProperty("billing_agreement_id", out var billingAgreement))
        {
            subscriptionId = billingAgreement.GetString();
        }

        mLogger.LogInformation(
            "Payment completed: {TransactionId}, Amount: {Amount} {Currency}, Subscription: {SubscriptionId}",
            transactionId, amount, currency, subscriptionId);

        // 1. Check for duplicate transaction (idempotency)
        // 2. Record payment in your database
        // 3. Update subscription billing cycle count and total paid
        // 4. Send payment receipt email

        await Task.CompletedTask;
    }

    private async Task HandlePaymentDenied(JsonDocument webhookEvent)
    {
        var resource = webhookEvent.RootElement.GetProperty("resource");
        var transactionId = resource.GetProperty("id").GetString();

        String? subscriptionId = null;
        if (resource.TryGetProperty("billing_agreement_id", out var billingAgreement))
        {
            subscriptionId = billingAgreement.GetString();
        }

        mLogger.LogWarning(
            "Payment denied: {TransactionId}, Subscription: {SubscriptionId}",
            transactionId, subscriptionId);

        // 1. Record failed payment
        // 2. Update subscription payment status to "failed"
        // 3. Send payment failure notification email

        await Task.CompletedTask;
    }

    private async Task HandlePaymentRefunded(JsonDocument webhookEvent)
    {
        var resource = webhookEvent.RootElement.GetProperty("resource");
        var transactionId = resource.GetProperty("id").GetString();

        mLogger.LogInformation("Payment refunded: {TransactionId}", transactionId);

        // 1. Record refund transaction
        // 2. Update original payment status

        await Task.CompletedTask;
    }
}
```

## Webhook Event Types Reference

### Subscription Events

| Event Type | Description | Key Data |
|---|---|---|
| `BILLING.SUBSCRIPTION.ACTIVATED` | Subscription started | `resource.id` = subscription ID |
| `BILLING.SUBSCRIPTION.UPDATED` | Subscription details changed | `resource.id`, updated fields |
| `BILLING.SUBSCRIPTION.SUSPENDED` | Subscription paused | `resource.id` |
| `BILLING.SUBSCRIPTION.CANCELLED` | Subscription ended | `resource.id` |
| `BILLING.SUBSCRIPTION.EXPIRED` | Subscription reached end | `resource.id` |

### Payment Events

| Event Type | Description | Key Data |
|---|---|---|
| `PAYMENT.SALE.COMPLETED` | Payment received | `resource.id` = transaction ID, `resource.amount`, `resource.billing_agreement_id` |
| `PAYMENT.SALE.DENIED` | Payment failed | `resource.id`, failure details |
| `PAYMENT.SALE.REFUNDED` | Payment refunded | `resource.id`, refund amount |

## PayPal Dashboard Webhook Setup

1. Go to [developer.paypal.com](https://developer.paypal.com) > Apps & Credentials
2. Select your REST API App
3. Scroll to Webhooks section
4. Click "Add Webhook"
5. Enter your webhook URL: `https://yourdomain.com/api/paypal/webhooks`
6. Select event types:
   - All events under "Billing subscription"
   - All events under "Payment sale"
7. Save and copy the **Webhook ID**
8. Set `PAYPAL_WEBHOOK_ID` environment variable

## Testing Webhooks

### Using PayPal Sandbox

1. Use sandbox test accounts to trigger real events
2. Check PayPal Dashboard > Webhooks > Recent Events for delivery status
3. Re-send failed events from the dashboard

### Using PayPal Webhook Simulator

1. Go to [developer.paypal.com](https://developer.paypal.com) > Dashboard > Testing Tools > Webhooks Simulator
2. Select your webhook URL
3. Choose event type to simulate
4. Click "Send Test"

### Local Development with ngrok

For local testing, use ngrok to expose your local server:

```bash
ngrok http 5000
```

Then use the ngrok HTTPS URL as your webhook URL in PayPal Dashboard.

## Idempotency

Always check for duplicate events before processing:

```csharp
// Check if we already processed this transaction
var existingPayment = await mRepository.GetPaymentByTransactionIdAsync(transactionId);
if (existingPayment != null)
{
    mLogger.LogInformation("Duplicate payment event for {TransactionId}, skipping", transactionId);
    return;
}
```

## Security: Webhook Signature Verification

For production, verify webhook signatures using PayPal-provided headers:

```csharp
// Extract verification headers
var transmissionId   = Request.Headers["PayPal-Transmission-Id"].FirstOrDefault();
var transmissionTime = Request.Headers["PayPal-Transmission-Time"].FirstOrDefault();
var certUrl          = Request.Headers["PayPal-Cert-Url"].FirstOrDefault();
var authAlgo         = Request.Headers["PayPal-Auth-Algo"].FirstOrDefault();
var transmissionSig  = Request.Headers["PayPal-Transmission-Sig"].FirstOrDefault();

// Verify using PayPal's Verify Webhook Signature API
// POST https://api-m.paypal.com/v1/notifications/verify-webhook-signature
```
