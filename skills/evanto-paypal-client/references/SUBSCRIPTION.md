# Subscription Integration Reference

Complete implementation examples for PayPal recurring subscriptions.

## PayPal Dashboard Setup

Before implementing subscriptions in code, set up the following in the PayPal Developer Dashboard:

### 1. Create a Product

- Navigate to: Dashboard > Billing > Products
- Create a product of type `SERVICE`
- Note the **Product ID** (e.g., `PROD-XXXX`)

### 2. Create a Plan

- Navigate to: Dashboard > Billing > Plans
- Select your product
- Configure billing cycle:
  - **Monthly**: Interval unit = MONTH, Interval count = 1
  - **Yearly**: Interval unit = YEAR, Interval count = 1
- Set the fixed price (e.g., `9.90 EUR`)
- Note the **Plan ID** (e.g., `P-1AB23456CD789012E...`)

### 3. Create Sandbox Test Accounts

- Navigate to: Dashboard > Sandbox > Accounts
- Create a Business account (merchant/seller)
- Create a Personal account (buyer/subscriber)
- Use these accounts to test subscription flows

## Backend: Subscription Controller

```csharp
using Evanto.PayPal.Client.Contracts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/paypal")]
public class PayPalSubscriptionController : ControllerBase
{
    private readonly IEvPayPalClient mPayPalClient;
    private readonly ILogger<PayPalSubscriptionController> mLogger;

    public PayPalSubscriptionController(
        IEvPayPalClient payPalClient,
        ILogger<PayPalSubscriptionController> logger)
    {
        mPayPalClient = payPalClient;
        mLogger       = logger;
    }

    /// <summary>   Processes subscription after customer approval in PayPal popup. </summary>
    [HttpPost("approve-subscription")]
    public async Task<ActionResult> ApproveSubscription([FromBody] SubscriptionApprovalRequest request)
    {
        if (!ModelState.IsValid || request == null)
        {
            return BadRequest(new { success = false, error = "Invalid request" });
        }

        try
        {   // 1. Validate subscription exists in PayPal
            var paypalSubscription = await mPayPalClient.GetSubscriptionAsync(request.SubscriptionId);
            if (!paypalSubscription.Success)
            {
                return BadRequest(new { success = false, error = "PayPal subscription not found" });
            }

            // 2. Create subscription record in your system
            // var subscription = await mSubscriptionService.CreateAsync(
            //     request.SubscriptionId,
            //     request.PlanId,
            //     request.BillingAddress,
            //     paypalSubscription.NextBillingTime);

            mLogger.LogInformation("Approved subscription {SubscriptionId}", request.SubscriptionId);

            return Ok(new { success = true, subscriptionId = request.SubscriptionId });
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to approve subscription {Id}", request.SubscriptionId);
            return StatusCode(500, new { success = false, error = "Failed to process subscription" });
        }
    }

    /// <summary>   Gets subscription details. </summary>
    [HttpGet("subscriptions/{subscriptionId}")]
    public async Task<ActionResult> GetSubscription(String subscriptionId)
    {
        try
        {
            var subscription = await mPayPalClient.GetSubscriptionAsync(subscriptionId);
            if (!subscription.Success)
            {
                return NotFound(new { error = "Subscription not found" });
            }

            return Ok(subscription);
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to get subscription {Id}", subscriptionId);
            return StatusCode(500, new { error = "Failed to get subscription" });
        }
    }

    /// <summary>   Suspends a subscription. </summary>
    [HttpPost("subscriptions/{subscriptionId}/suspend")]
    public async Task<ActionResult> SuspendSubscription(
        String subscriptionId,
        [FromBody] SubscriptionActionRequest request)
    {
        try
        {
            var suspended = await mPayPalClient.SuspendSubscriptionAsync(
                subscriptionId,
                request?.Reason ?? "Customer request");

            if (!suspended)
            {
                return StatusCode(500, new { error = "Failed to suspend subscription in PayPal" });
            }

            // Update local record status to "suspended"
            return Ok(new { success = true, message = "Subscription suspended" });
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to suspend subscription {Id}", subscriptionId);
            return StatusCode(500, new { error = "Failed to suspend subscription" });
        }
    }

    /// <summary>   Cancels a subscription. </summary>
    [HttpPost("subscriptions/{subscriptionId}/cancel")]
    public async Task<ActionResult> CancelSubscription(
        String subscriptionId,
        [FromBody] SubscriptionActionRequest request)
    {
        try
        {
            var cancelled = await mPayPalClient.CancelSubscriptionAsync(
                subscriptionId,
                request?.Reason ?? "Customer request");

            if (!cancelled)
            {
                return StatusCode(500, new { error = "Failed to cancel subscription in PayPal" });
            }

            // Update local record: status = "cancelled", endDate = DateTime.UtcNow
            return Ok(new { success = true, message = "Subscription cancelled" });
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to cancel subscription {Id}", subscriptionId);
            return StatusCode(500, new { error = "Failed to cancel subscription" });
        }
    }

    /// <summary>   Reactivates a suspended subscription. </summary>
    [HttpPost("subscriptions/{subscriptionId}/activate")]
    public async Task<ActionResult> ActivateSubscription(
        String subscriptionId,
        [FromBody] SubscriptionActionRequest request)
    {
        try
        {
            var activated = await mPayPalClient.ActivateSubscriptionAsync(
                subscriptionId,
                request?.Reason ?? "Customer request");

            if (!activated)
            {
                return StatusCode(500, new { error = "Failed to activate subscription" });
            }

            // Sync status from PayPal to get updated billing info
            return Ok(new { success = true, message = "Subscription activated" });
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to activate subscription {Id}", subscriptionId);
            return StatusCode(500, new { error = "Failed to activate subscription" });
        }
    }
}

// Request models
public class SubscriptionApprovalRequest
{
    public String SubscriptionId { get; set; } = String.Empty;
    public String PlanId { get; set; } = String.Empty;
    public BillingAddressModel? BillingAddress { get; set; }
    public String? Password { get; set; }
}

public class SubscriptionActionRequest
{
    public String? Reason { get; set; }
}

public class BillingAddressModel
{
    public String FirstName { get; set; } = String.Empty;
    public String LastName { get; set; } = String.Empty;
    public String Email { get; set; } = String.Empty;
    public String Street { get; set; } = String.Empty;
    public String PostalCode { get; set; } = String.Empty;
    public String City { get; set; } = String.Empty;
    public String Country { get; set; } = "DE";
}
```

## Frontend: Subscription Checkout Page (Razor)

### PayPal SDK Script Tag

```cshtml
@section Scripts {
    @{
        var paypalClientId = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID");
        // Note: intent=subscription and vault=true are required for subscriptions
        var paypalSdkUrl = $"https://www.paypal.com/sdk/js?client-id={paypalClientId}&currency=EUR&intent=subscription&vault=true";
    }
    <script src="@paypalSdkUrl"></script>
}
```

### Subscription Checkout HTML

```html
<!-- Hidden fields with plan information -->
<input type="hidden" id="plan-id" value="@Model.PlanId" />
<input type="hidden" id="paypal-plan-id" value="@Model.PayPalPlanId" />

<!-- Billing address form -->
<form id="subscription-checkout-form">
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
            <label for="BillingAddress_FirstName">First Name *</label>
            <input type="text" id="BillingAddress_FirstName" required />
        </div>
        <div>
            <label for="BillingAddress_LastName">Last Name *</label>
            <input type="text" id="BillingAddress_LastName" required />
        </div>
        <div class="md:col-span-2">
            <label for="BillingAddress_Email">Email *</label>
            <input type="email" id="BillingAddress_Email" required />
        </div>
        <div class="md:col-span-2">
            <label for="BillingAddress_Street">Street *</label>
            <input type="text" id="BillingAddress_Street" required />
        </div>
        <div>
            <label for="BillingAddress_PostalCode">Postal Code *</label>
            <input type="text" id="BillingAddress_PostalCode" required />
        </div>
        <div>
            <label for="BillingAddress_City">City *</label>
            <input type="text" id="BillingAddress_City" required />
        </div>
        <div class="md:col-span-2">
            <label for="BillingAddress_Country">Country *</label>
            <select id="BillingAddress_Country" required>
                <option value="DE" selected>Germany</option>
                <option value="AT">Austria</option>
                <option value="CH">Switzerland</option>
            </select>
        </div>
    </div>
</form>

<!-- PayPal Subscription Button -->
<div id="paypal-button-container"></div>

<!-- Order Summary -->
<div class="order-summary">
    <h3>@Model.PlanName</h3>
    <p>@Model.PlanPrice.ToString("N2") EUR / month</p>
</div>
```

### Subscription JavaScript Integration

```javascript
document.addEventListener('DOMContentLoaded', function() {
    const paypalPlanId = document.getElementById('paypal-plan-id').value;

    if (typeof paypal === 'undefined' || !paypalPlanId) {
        const container = document.getElementById('paypal-button-container');
        const errorMsg = document.createElement('p');
        errorMsg.className = 'text-red-500';
        errorMsg.textContent = 'PayPal is currently unavailable.';
        container.appendChild(errorMsg);
        return;
    }

    // Form validation helper
    function validateForm() {
        const form = document.getElementById('subscription-checkout-form');
        const inputs = form.querySelectorAll('input[required], select[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!input.value.trim()) {
                isValid = false;
                input.classList.add('border-red-500');
            } else {
                input.classList.remove('border-red-500');
            }
        });

        // Email validation
        const emailInput = document.getElementById('BillingAddress_Email');
        if (emailInput && emailInput.value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(emailInput.value)) {
                isValid = false;
                emailInput.classList.add('border-red-500');
            }
        }

        return isValid;
    }

    // Get form data helper
    function getFormData() {
        return {
            firstName:  document.getElementById('BillingAddress_FirstName').value,
            lastName:   document.getElementById('BillingAddress_LastName').value,
            email:      document.getElementById('BillingAddress_Email').value,
            street:     document.getElementById('BillingAddress_Street').value,
            postalCode: document.getElementById('BillingAddress_PostalCode').value,
            city:       document.getElementById('BillingAddress_City').value,
            country:    document.getElementById('BillingAddress_Country').value,
            planId:     document.getElementById('plan-id').value
        };
    }

    paypal.Buttons({
        style: {
            shape:  'rect',
            color:  'gold',
            layout: 'vertical',
            label:  'subscribe'
        },

        // Creates the subscription in PayPal
        createSubscription: function(data, actions) {
            if (!validateForm()) {
                return Promise.reject(new Error('Form validation failed'));
            }

            return actions.subscription.create({
                plan_id: paypalPlanId
            });
        },

        // Called after customer approves the subscription in PayPal popup
        onApprove: async function(data, actions) {
            const formData = getFormData();

            try {
                const response = await fetch('/api/paypal/approve-subscription', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        subscriptionId: data.subscriptionID,
                        planId: formData.planId,
                        billingAddress: {
                            firstName:  formData.firstName,
                            lastName:   formData.lastName,
                            email:      formData.email,
                            street:     formData.street,
                            postalCode: formData.postalCode,
                            city:       formData.city,
                            country:    formData.country
                        }
                    })
                });

                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.error || 'Failed to process subscription');
                }

                // Redirect to confirmation
                window.location.href = `/subscription/confirmation?subscriptionId=${data.subscriptionID}`;

            } catch (error) {
                console.error('Subscription approval error:', error);
                // Show error to user via DOM
            }
        },

        onCancel: function(data) {
            console.log('Subscription cancelled by user');
        },

        onError: function(err) {
            console.error('PayPal error:', err);
        }
    }).render('#paypal-button-container');
});
```

## Plan Management (Server-Side)

### Creating Plans Programmatically

```csharp
using Evanto.PayPal.Client.Contracts;
using Evanto.PayPal.Client.Models;

// Create a monthly subscription plan
var planRequest = new EvPayPalPlanRequest
{
    ProductId   = "PROD-12345",          // From PayPal Dashboard
    Name        = "Premium Monthly",
    Description = "Monthly premium membership with full access",
    BillingCycles = new List<EvPayPalBillingCycle>
    {
        new()
        {
            TenureType      = "REGULAR",  // TRIAL or REGULAR
            Sequence        = 1,
            TotalCycles     = 0,          // 0 = infinite billing
            IntervalUnit    = "MONTH",    // DAY, WEEK, MONTH, YEAR
            IntervalCount   = 1,          // Every 1 month
            FixedPriceValue = 9.90m,
            CurrencyCode    = "EUR"
        }
    },
    AutoBillOutstanding     = true,       // Retry failed payments
    SetupFeeValue           = 0m,         // No setup fee
    SetupFeeFailureAction   = "CONTINUE", // CONTINUE or CANCEL on setup fee failure
    PaymentFailureThreshold = 3           // Max failures before suspension
};

var plan = await mPayPalClient.CreatePlanAsync(planRequest);

if (plan.Success)
{
    // Plan ID: plan.PlanId (e.g., "P-1AB23456CD789012E...")
    // Activate the plan
    await mPayPalClient.ActivatePlanAsync(plan.PlanId);
}
```

### Plan with Trial Period

```csharp
var planRequest = new EvPayPalPlanRequest
{
    ProductId   = "PROD-12345",
    Name        = "Premium with Trial",
    Description = "14-day free trial, then monthly billing",
    BillingCycles = new List<EvPayPalBillingCycle>
    {
        // Trial period (14 days free)
        new()
        {
            TenureType      = "TRIAL",
            Sequence        = 1,
            TotalCycles     = 1,          // 1 trial cycle
            IntervalUnit    = "DAY",
            IntervalCount   = 14,         // 14 days
            FixedPriceValue = 0m,         // Free trial
            CurrencyCode    = "EUR"
        },
        // Regular billing after trial
        new()
        {
            TenureType      = "REGULAR",
            Sequence        = 2,
            TotalCycles     = 0,          // Infinite
            IntervalUnit    = "MONTH",
            IntervalCount   = 1,
            FixedPriceValue = 9.90m,
            CurrencyCode    = "EUR"
        }
    }
};
```

## Subscription Lifecycle Management

### From a Profile/Admin Page

```csharp
// Suspend subscription
var suspended = await mPayPalClient.SuspendSubscriptionAsync(
    subscriptionId: "I-1234567890",
    reason: "Customer requested pause");

// Cancel subscription
var cancelled = await mPayPalClient.CancelSubscriptionAsync(
    subscriptionId: "I-1234567890",
    reason: "Customer cancellation");

// Reactivate suspended subscription
var activated = await mPayPalClient.ActivateSubscriptionAsync(
    subscriptionId: "I-1234567890",
    reason: "Customer reactivation request");

// Get transaction history
var transactions = await mPayPalClient.GetSubscriptionTransactionsAsync(
    subscriptionId: "I-1234567890",
    startTime: DateTime.UtcNow.AddMonths(-6),
    endTime: DateTime.UtcNow);

if (transactions.Success)
{
    foreach (var tx in transactions.Transactions)
    {
        // tx.TransactionId, tx.Amount, tx.Status, tx.Time
    }
}
```

## Subscription Response Model

```csharp
public class EvPayPalSubscriptionResponse
{
    public Boolean Success { get; set; }
    public String? SubscriptionId { get; set; }
    public String? Status { get; set; }         // APPROVAL_PENDING, APPROVED, ACTIVE, SUSPENDED, CANCELLED, EXPIRED
    public String? PlanId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? NextBillingTime { get; set; }
    public String? SubscriberEmail { get; set; }
    public String? SubscriberName { get; set; }
    public String? ApprovalUrl { get; set; }
    public String? ErrorMessage { get; set; }
}
```
