---
name: evanto-paypal-client
description: Integrate PayPal payments into .NET projects using Evanto.PayPal.Client. Covers one-time order payments, recurring subscriptions, fee calculation, webhook handling, and frontend SDK integration. Use when adding PayPal checkout, subscription billing, or payment processing to a new or existing project.
license: Proprietary
metadata:
  author: evanto
  version: "1.0"
compatibility: Requires .NET 10.0+ and PayPal Server SDK (paypal-server-sdk 2.1.0). Requires PayPal developer account with REST API credentials.
---

# Evanto PayPal Client Integration Skill

This skill guides you in integrating PayPal payments into .NET projects using the `Evanto.PayPal.Client` library. It covers **one-time order payments**, **recurring subscriptions** (via PayPal), and **SEPA Direct Debit** as an alternative subscription payment method.

## Prerequisites

1. **PayPal Developer Account** at [developer.paypal.com](https://developer.paypal.com)
2. **REST API App** created in PayPal Dashboard (sandbox and/or live)
3. **NuGet Package**: `PaypalServerSdk` v2.1.0+
4. **Project Reference**: `Evanto.PayPal.Client` library at `evanto/paypal/src/Evanto.PayPal.Client/`

## When to Use This Skill

- Adding PayPal checkout to an e-commerce flow
- Implementing recurring subscription billing via PayPal
- Adding SEPA Direct Debit as an alternative subscription payment method
- Adding PayPal fee calculation to order totals
- Setting up PayPal webhook handlers for payment/subscription events
- Integrating the PayPal JavaScript SDK on checkout pages

## Configuration

### Environment Variables

The following environment variables configure PayPal integration:

| Variable | Description | Example |
|---|---|---|
| `PAYPAL_CLIENT_ID` | REST API Client ID | `AaBb...sandbox-id` |
| `PAYPAL_CLIENT_SECRET` | REST API Client Secret | `EeFf...sandbox-secret` |
| `PAYPAL_ENVIRONMENT` | `sandbox` or `live` / `production` | `sandbox` |
| `PAYPAL_WEBHOOK_ID` | Webhook ID from PayPal Dashboard | `5GP02...` |

### Sandbox vs Live

| Setting | Sandbox | Live |
|---|---|---|
| `PAYPAL_ENVIRONMENT` | `sandbox` | `live` or `production` |
| `PAYPAL_CLIENT_ID` | Sandbox app Client ID | Live app Client ID |
| `PAYPAL_CLIENT_SECRET` | Sandbox app Client Secret | Live app Client Secret |
| `PAYPAL_WEBHOOK_ID` | Sandbox webhook ID | Live webhook ID |
| PayPal JS SDK URL | Uses sandbox client ID | Uses live client ID |
| Test accounts | Use sandbox buyer/seller accounts | Real PayPal accounts |

### appsettings.json Configuration

```json
{
  "PayPal": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "Environment": "sandbox",
    "WebhookId": "YOUR_WEBHOOK_ID",
    "PayPalDisabled": false
  },
  "PayPalFees": {
    "FixedFeeAmount": 0.39,
    "PercentageFeeRate": 2.99,
    "IsEnabled": true,
    "MinimumFeeAmount": null,
    "MaximumFeeAmount": null
  }
}
```

## Adding PayPal to a Project

### Step 1: Add Project Reference

```xml
<ProjectReference Include="..\..\evanto\paypal\src\Evanto.PayPal.Client\Evanto.PayPal.Client.csproj" />
```

### Step 2: Register Services in Program.cs

```csharp
using Evanto.PayPal.Client.Extensions;
using Evanto.PayPal.Client.Settings;

// Bind PayPal settings from configuration or environment
var payPalSettings = new EvPayPalSettings
{
    ClientId     = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID") ?? configuration["PayPal:ClientId"] ?? String.Empty,
    ClientSecret = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_SECRET") ?? configuration["PayPal:ClientSecret"] ?? String.Empty,
    Environment  = Environment.GetEnvironmentVariable("PAYPAL_ENVIRONMENT") ?? configuration["PayPal:Environment"] ?? "sandbox",
    WebhookId    = Environment.GetEnvironmentVariable("PAYPAL_WEBHOOK_ID") ?? configuration["PayPal:WebhookId"] ?? String.Empty
};

// Optional: fee settings for PayPal surcharge calculation
var payPalFeeSettings = new EvPayPalFeeSettings
{
    FixedFeeAmount    = 0.39m,
    PercentageFeeRate = 2.99m,
    IsEnabled         = true
};

// Register all PayPal services (IEvPayPalClient + optional IEvPayPalFeeCalculationService)
builder.Services.AddEvPayPalPayments(payPalSettings, payPalFeeSettings);
```

The `AddEvPayPalPayments` extension method registers:
- `EvPayPalSettings` as singleton
- `EvPayPalFeeSettings` as singleton (if provided)
- `IEvPayPalClient` as scoped service
- `IEvPayPalFeeCalculationService` as scoped service (if fee settings provided)

### Step 3: Inject and Use

```csharp
public class MyController(IEvPayPalClient payPalClient, ILogger<MyController> logger) : ControllerBase
{
    private readonly IEvPayPalClient mPayPalClient = payPalClient;
    private readonly ILogger<MyController> mLogger = logger;
}
```

## One-Time Payment Integration

### API Endpoints Pattern

Create an API controller with two endpoints:

1. **POST `/api/paypal/create-order`** - Creates a PayPal order
2. **POST `/api/paypal/capture-order/{paypalOrderId}`** - Captures (completes) the payment

See [references/ONE_TIME_PAYMENT.md](references/ONE_TIME_PAYMENT.md) for the complete controller and frontend implementation.

### Backend Flow

```csharp
// 1. Create order with total amount
var response = await mPayPalClient.CreateOrderAsync(
    total: cart.Total,           // Decimal amount in EUR
    returnUrl: "https://example.com/checkout?status=success",
    cancelUrl: "https://example.com/checkout?status=cancel"
);
// response.Id = PayPal order ID
// response.ApprovalUrl = URL to redirect customer (server-side flow)

// 2. After customer approves, capture the payment
var capture = await mPayPalClient.CaptureOrderAsync(paypalOrderId);
// capture.Status == "COMPLETED" means payment successful
```

### Frontend Flow (PayPal JS SDK)

Load the PayPal JavaScript SDK with `intent=capture` for one-time payments:

```html
<script src="https://www.paypal.com/sdk/js?client-id=YOUR_CLIENT_ID&currency=EUR&components=buttons&intent=capture"></script>
```

Render PayPal buttons:

```javascript
paypal.Buttons({
    createOrder: async function(data, actions) {
        const response = await fetch('/api/paypal/create-order', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ returnUrl: window.location.href, cancelUrl: window.location.href })
        });
        const order = await response.json();
        return order.id;    // Return PayPal order ID to SDK
    },
    onApprove: async function(data, actions) {
        const response = await fetch(`/api/paypal/capture-order/${data.orderID}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ billingAddress: { /* address fields */ }, shippingMethod: 'standard' })
        });
        const result = await response.json();
        if (result.status === 'COMPLETED') {
            window.location.href = '/order-confirmation?id=' + result.orderId;
        }
    },
    onError: function(err) {
        console.error('PayPal error:', err);
    }
}).render('#paypal-button-container');
```

## Subscription Integration

### PayPal Dashboard Setup (One-Time)

Before creating subscriptions in code:

1. **Create a Product** in PayPal Dashboard (Dashboard > Billing > Products)
2. **Create a Plan** for that product (monthly, yearly, etc.)
3. Note the **Plan ID** (e.g., `P-1AB23456CD789012E...`)

### API Endpoints Pattern

1. **POST `/api/paypal/approve-subscription`** - Processes subscription after customer approval
2. **GET `/api/paypal/subscriptions/{id}`** - Gets subscription details
3. **POST `/api/paypal/subscriptions/{id}/suspend`** - Suspends a subscription
4. **POST `/api/paypal/subscriptions/{id}/cancel`** - Cancels a subscription
5. **POST `/api/paypal/subscriptions/{id}/activate`** - Reactivates a suspended subscription

See [references/SUBSCRIPTION.md](references/SUBSCRIPTION.md) for the complete controller and frontend implementation.

### Backend Flow

```csharp
// Create subscription (if needed server-side)
var response = await mPayPalClient.CreateSubscriptionAsync(new EvPayPalSubscriptionRequest
{
    PlanId   = "P-1AB23456CD789012E...",
    CustomId = "internal-tracking-id"
});
// response.ApprovalUrl = redirect customer to approve

// Get subscription details
var sub = await mPayPalClient.GetSubscriptionAsync(subscriptionId);

// Lifecycle management
await mPayPalClient.SuspendSubscriptionAsync(subscriptionId, "Customer request");
await mPayPalClient.CancelSubscriptionAsync(subscriptionId, "Customer request");
await mPayPalClient.ActivateSubscriptionAsync(subscriptionId, "Customer request");

// Transaction history
var transactions = await mPayPalClient.GetSubscriptionTransactionsAsync(
    subscriptionId,
    DateTime.UtcNow.AddMonths(-12),
    DateTime.UtcNow);
```

### Frontend Flow (PayPal JS SDK)

Load the PayPal JavaScript SDK with `intent=subscription&vault=true` for subscriptions:

```html
<script src="https://www.paypal.com/sdk/js?client-id=YOUR_CLIENT_ID&currency=EUR&intent=subscription&vault=true"></script>
```

Render subscription buttons:

```javascript
paypal.Buttons({
    style: { shape: 'rect', color: 'gold', layout: 'vertical', label: 'subscribe' },
    createSubscription: function(data, actions) {
        return actions.subscription.create({
            plan_id: 'P-1AB23456CD789012E...'    // PayPal Plan ID
        });
    },
    onApprove: async function(data, actions) {
        // data.subscriptionID contains the PayPal subscription ID
        const response = await fetch('/api/paypal/approve-subscription', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                subscriptionId: data.subscriptionID,
                planId: 'internal-plan-id',
                billingAddress: { /* address fields */ }
            })
        });
        const result = await response.json();
        if (result.success) {
            window.location.href = '/subscription/confirmation?id=' + data.subscriptionID;
        }
    },
    onCancel: function(data) { /* handle cancellation */ },
    onError: function(err) { console.error('PayPal error:', err); }
}).render('#paypal-button-container');
```

## SEPA Direct Debit Integration

SEPA Direct Debit is an alternative subscription payment method offered alongside PayPal on the same checkout page. It requires an IBAN validation service and a subscription processing service in your application layer.

### API Endpoints Pattern

1. **POST `/api/subscription/approve-sepa`** - Creates a SEPA subscription with IBAN mandate
2. **GET `/api/subscription/validate-iban?iban=...`** - Validates an IBAN and returns country info
3. **GET `/api/subscription/sepa-countries?culture=de`** - Lists SEPA zone countries

See [references/SEPA.md](references/SEPA.md) for the complete controller and frontend implementation.

### Backend Flow

```csharp
// 1. Validate IBAN (implement ISepaValidationService in your app layer, e.g. using IbanNet)
var (isValid, error, countryCode) = mSepaValidationService.ValidateIban(iban);

// 2. Check SEPA country availability (e.g. using Nager.Country)
var isSepaCountry = mSepaValidationService.IsSepaCountry(countryCode);

// 3. Mask IBAN for display (e.g., "DE89 **** **** **** **** 00")
var maskedIban = mSepaValidationService.MaskIban(iban);

// 4. Process SEPA subscription (stores full IBAN + masked IBAN, creates mandate)
var response = await mSubscriptionService.ProcessSepaSubscriptionAsync(request, verificationUrl, baseUrl);
```

### Frontend Flow

The subscription checkout page offers both PayPal and SEPA as payment methods. When the customer selects SEPA:

1. Show SEPA form fields (account holder, IBAN, BIC, mandate consent)
2. Validate IBAN in real-time via `/api/subscription/validate-iban`
3. Check SEPA availability based on selected country
4. Submit via `/api/subscription/approve-sepa`

### SEPA Country Availability

SEPA Direct Debit is available in 41 European countries. Your SEPA validation service should expose an `IsSepaCountry(countryCode)` method that checks if a country code is in the SEPA zone. The checkout page dynamically enables/disables the SEPA option based on the customer's selected billing country.

### SEPA Mandate

Each SEPA subscription generates a mandate reference in the format `MANDATE-{date}-{guid}`. The customer must explicitly consent to the direct debit mandate via a checkbox before the subscription can be created.

## Webhook Handling

### Setup in PayPal Dashboard

1. Go to Apps & Credentials > Your App > Webhooks
2. Add webhook URL: `https://yourdomain.com/api/paypal/webhooks`
3. Select event types:
   - `BILLING.SUBSCRIPTION.ACTIVATED`
   - `BILLING.SUBSCRIPTION.UPDATED`
   - `BILLING.SUBSCRIPTION.SUSPENDED`
   - `BILLING.SUBSCRIPTION.CANCELLED`
   - `BILLING.SUBSCRIPTION.EXPIRED`
   - `PAYMENT.SALE.COMPLETED`
   - `PAYMENT.SALE.DENIED`
   - `PAYMENT.SALE.REFUNDED`
4. Copy Webhook ID to `PAYPAL_WEBHOOK_ID` environment variable

### Webhook Controller Pattern

```csharp
[ApiController]
[Route("api/paypal/webhooks")]
public class PayPalWebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var webhookEvent = JsonDocument.Parse(body);

        var eventType = webhookEvent.RootElement.GetProperty("event_type").GetString();

        switch (eventType)
        {
            case "BILLING.SUBSCRIPTION.ACTIVATED":
                // Mark subscription active, grant access
                break;
            case "PAYMENT.SALE.COMPLETED":
                // Record payment, update billing cycle
                break;
            case "BILLING.SUBSCRIPTION.CANCELLED":
                // Mark cancelled, revoke access
                break;
            // Handle other events...
        }

        return Ok();    // Always return 200 to acknowledge receipt
    }
}
```

See [references/WEBHOOK.md](references/WEBHOOK.md) for the complete webhook handler implementation.

## Fee Calculation

The `IEvPayPalFeeCalculationService` calculates PayPal transaction fees to optionally pass on to customers:

```csharp
// Inject the service
private readonly IEvPayPalFeeCalculationService mFeeService;

// Calculate fee
var fee = await mFeeService.CalculateFeeAsync(
    subtotal: 100.00m,
    shippingCost: 5.95m
);
// Formula: FixedFee + (Subtotal + Shipping) * (PercentageRate / 100)
// Example: 0.39 + 105.95 * 0.0299 = 3.57 EUR
```

## Plan Management (Server-Side)

For programmatic plan creation (optional; plans can also be created in PayPal Dashboard):

```csharp
// Create a billing plan
var plan = await mPayPalClient.CreatePlanAsync(new EvPayPalPlanRequest
{
    ProductId   = "PROD-12345",
    Name        = "Premium Monthly",
    Description = "Monthly premium membership",
    BillingCycles = new List<EvPayPalBillingCycle>
    {
        new()
        {
            TenureType     = "REGULAR",
            Sequence       = 1,
            TotalCycles    = 0,          // 0 = infinite
            IntervalUnit   = "MONTH",
            IntervalCount  = 1,
            FixedPriceValue = 9.90m,
            CurrencyCode   = "EUR"
        }
    },
    AutoBillOutstanding     = true,
    SetupFeeValue           = 0m,
    PaymentFailureThreshold = 3
});

// Activate the plan
await mPayPalClient.ActivatePlanAsync(plan.PlanId);
```

## API Reference

### IEvPayPalClient Interface

| Method | Description |
|---|---|
| `CreateOrderAsync(total, returnUrl, cancelUrl)` | Creates a one-time payment order |
| `CaptureOrderAsync(paypalOrderId)` | Captures/completes a payment |
| `CreateSubscriptionAsync(request)` | Creates a subscription |
| `GetSubscriptionAsync(subscriptionId)` | Gets subscription details |
| `SuspendSubscriptionAsync(id, reason)` | Suspends a subscription |
| `CancelSubscriptionAsync(id, reason)` | Cancels a subscription |
| `ActivateSubscriptionAsync(id, reason)` | Reactivates a suspended subscription |
| `GetSubscriptionTransactionsAsync(id, start, end)` | Gets transaction history |
| `CreatePlanAsync(request)` | Creates a billing plan |
| `GetPlanAsync(planId)` | Gets plan details |
| `ActivatePlanAsync(planId)` | Activates a plan |
| `DeactivatePlanAsync(planId)` | Deactivates a plan |

### Response Models

| Model | Key Properties |
|---|---|
| `EvPayPalCreateOrderResponse` | `Id`, `Status`, `ApprovalUrl` |
| `EvPayPalCaptureOrderResponse` | `Id`, `Status`, `OrderId` |
| `EvPayPalSubscriptionResponse` | `Success`, `SubscriptionId`, `Status`, `PlanId`, `ApprovalUrl`, `NextBillingTime` |
| `EvPayPalPlanResponse` | `Success`, `PlanId`, `Name`, `Status` |
| `EvPayPalTransactionListResponse` | `Success`, `Transactions[]` |

## Common Patterns

### Error Handling

All client methods throw on infrastructure errors and return error details in response objects:

```csharp
var response = await mPayPalClient.CreateSubscriptionAsync(request);
if (!response.Success)
{
    mLogger.LogWarning("Subscription creation failed: {Error}", response.ErrorMessage);
    return BadRequest(new { error = response.ErrorMessage });
}
```

### PayPal SDK URL Construction

```csharp
// One-time payments
var sdkUrl = $"https://www.paypal.com/sdk/js?client-id={clientId}&currency=EUR&components=buttons&intent=capture";

// Subscriptions
var sdkUrl = $"https://www.paypal.com/sdk/js?client-id={clientId}&currency=EUR&intent=subscription&vault=true";
```

## Troubleshooting

| Issue | Solution |
|---|---|
| PayPal buttons don't render | Check `PAYPAL_CLIENT_ID` is set and PayPal JS SDK loads without errors |
| "INSTRUMENT_DECLINED" error | Sandbox: use sandbox test buyer account. Live: customer's payment method was declined |
| Webhook events not received | Verify webhook URL is publicly accessible and HTTPS. Check PayPal Dashboard webhook logs |
| Order capture returns non-COMPLETED | Customer may not have completed approval. Check `ApprovalUrl` redirect flow |
| Fee calculation returns 0 | Verify `EvPayPalFeeSettings.IsEnabled` is true |

## Source Code Reference

- **Client Library**: `evanto/paypal/src/Evanto.PayPal.Client/`
- **Extension Method**: `evanto/paypal/src/Evanto.PayPal.Client/Extensions/EvPayPalExtensions.cs`
- **Settings**: `evanto/paypal/src/Evanto.PayPal.Client/Settings/EvPayPalSettings.cs`
- **Fee Settings**: `evanto/paypal/src/Evanto.PayPal.Client/Settings/EvPayPalFeeSettings.cs`

In the consuming web project, create these controllers and pages as needed:

- **Payment Controller**: `Controllers/Api/PayPalController.cs`
- **Subscription Controller (SEPA)**: `Controllers/Api/SubscriptionController.cs`
- **Webhook Controller**: `Controllers/Api/PayPalWebhookController.cs`
- **Cart Checkout (one-time)**: `Pages/Cart/Checkout.cshtml`
- **Subscription Checkout**: `Pages/Subscription/Checkout.cshtml`
- **PayPal JS Integration**: `wwwroot/js/paypal-integration.js`
