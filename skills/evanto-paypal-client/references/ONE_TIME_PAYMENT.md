# One-Time Payment Integration Reference

Complete implementation examples for PayPal one-time (order) payments.

## Backend: PayPal Order Controller

```csharp
using Evanto.PayPal.Client.Contracts;
using Evanto.PayPal.Client.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PayPalController : ControllerBase
{
    private readonly IEvPayPalClient mPayPalClient;
    private readonly ILogger<PayPalController> mLogger;

    public PayPalController(IEvPayPalClient payPalClient, ILogger<PayPalController> logger)
    {
        mPayPalClient = payPalClient ?? throw new ArgumentNullException(nameof(payPalClient));
        mLogger       = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>   Creates a PayPal order from the current cart. </summary>
    [HttpPost("create-order")]
    public async Task<ActionResult<EvPayPalCreateOrderResponse>> CreateOrder(
        [FromBody] EvPayPalCreateOrderRequest request)
    {
        if (!ModelState.IsValid || request == null)
        {
            return BadRequest(ModelState);
        }

        try
        {   // Get cart total from your cart service
            var cartTotal = 99.90m; // Replace with actual cart total

            var response = await mPayPalClient.CreateOrderAsync(
                cartTotal,
                request.ReturnUrl,
                request.CancelUrl);

            mLogger.LogInformation("Created PayPal order {OrderId} for {Total}",
                response.Id, cartTotal);

            return Ok(response);
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "PayPal order creation failed");
            return StatusCode(500, new { error = "Failed to create PayPal order" });
        }
    }

    /// <summary>   Captures a PayPal order and creates an order in our system. </summary>
    [HttpPost("capture-order/{paypalOrderId}")]
    public async Task<ActionResult<EvPayPalCaptureOrderResponse>> CaptureOrder(
        String paypalOrderId,
        [FromBody] EvPayPalCaptureOrderRequest request)
    {
        if (String.IsNullOrWhiteSpace(paypalOrderId))
        {
            return BadRequest(new { error = "PayPal order ID is required" });
        }

        try
        {
            var response = await mPayPalClient.CaptureOrderAsync(paypalOrderId);

            if (String.Equals(response.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            {   // Payment successful - create order in your system
                // var order = await mOrderService.CreateOrderAsync(...);
                // response.OrderId = order.Id;

                mLogger.LogInformation("Captured PayPal order {OrderId}", paypalOrderId);
            }

            return Ok(response);
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to capture PayPal order {OrderId}", paypalOrderId);
            return StatusCode(500, new { error = "Failed to capture PayPal order" });
        }
    }
}
```

## Frontend: Checkout Page (Razor)

### PayPal SDK Script Tag

```cshtml
@section Scripts {
    @{
        var paypalClientId = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID");
        var paypalSdkUrl = $"https://www.paypal.com/sdk/js?client-id={paypalClientId}&currency=EUR&components=buttons&intent=capture";
    }
    <script src="@paypalSdkUrl"></script>
}
```

### PayPal Button Container (HTML)

```html
<!-- Payment method selection -->
<div class="space-y-4">
    <label class="block relative p-4 border rounded-lg cursor-pointer">
        <input type="radio" name="PaymentMethod" value="bank" checked class="sr-only" />
        <div class="flex items-center">
            <i class="ri-bank-line text-xl mr-2"></i>
            <div>
                <div class="font-medium">Bank Transfer</div>
                <div class="text-sm text-gray-500">Pay via bank transfer</div>
            </div>
        </div>
    </label>

    <label class="block relative p-4 border rounded-lg cursor-pointer">
        <input type="radio" name="PaymentMethod" value="paypal" class="sr-only" />
        <div class="flex items-center">
            <i class="ri-paypal-fill text-xl mr-2 text-[#003087]"></i>
            <div>
                <div class="font-medium">PayPal</div>
                <div class="text-sm text-gray-500">Fast and secure payment</div>
            </div>
        </div>
    </label>
</div>

<!-- PayPal button renders here -->
<div id="paypal-button-container"></div>
```

### PayPal JavaScript Integration

```javascript
document.addEventListener('DOMContentLoaded', function() {
    if (typeof paypal === 'undefined') {
        console.error('PayPal SDK not loaded');
        return;
    }

    paypal.Buttons({
        // Called when customer clicks PayPal button
        createOrder: async function(data, actions) {
            const response = await fetch('/api/paypal/create-order', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    returnUrl: window.location.href,
                    cancelUrl: window.location.href
                })
            });

            if (!response.ok) {
                throw new Error('Failed to create order');
            }

            const order = await response.json();
            return order.id;
        },

        // Called after customer approves payment in PayPal popup
        onApprove: async function(data, actions) {
            const billingAddress = {
                firstName:  document.getElementById('BillingAddress_FirstName').value,
                lastName:   document.getElementById('BillingAddress_LastName').value,
                email:      document.getElementById('BillingAddress_Email').value,
                street:     document.getElementById('BillingAddress_Street').value,
                postalCode: document.getElementById('BillingAddress_PostalCode').value,
                city:       document.getElementById('BillingAddress_City').value,
                country:    document.getElementById('BillingAddress_Country').value
            };

            const response = await fetch(`/api/paypal/capture-order/${data.orderID}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    billingAddress: billingAddress,
                    shippingMethod: 'standard',
                    notes: ''
                })
            });

            if (!response.ok) {
                throw new Error('Failed to capture order');
            }

            const result = await response.json();

            if (result.status === 'COMPLETED') {
                // Redirect to confirmation page
                window.location.href = '/order-confirmation?id=' + result.orderId;
            }
        },

        onCancel: function(data) {
            console.log('Payment cancelled');
        },

        onError: function(err) {
            console.error('PayPal error:', err);
            // Show error to user
        }
    }).render('#paypal-button-container');
});
```

## Fee Calculation Integration

To pass PayPal fees to the customer, inject `IEvPayPalFeeCalculationService`:

```csharp
public class CheckoutModel : PageModel
{
    private readonly IEvPayPalFeeCalculationService mFeeService;

    public async Task<IActionResult> OnPostCalculatePayPalFee()
    {
        var fee = await mFeeService.CalculateFeeAsync(
            subtotal: cart.Subtotal,
            shippingCost: cart.ShippingCost);

        return new JsonResult(new { fee = fee });
    }
}
```

Frontend AJAX call to update total with fee:

```javascript
async function updatePayPalFee(subtotal, shippingCost) {
    const response = await fetch('/api/cart/calculate-paypal-fee', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ subtotal, shippingCost })
    });
    const { fee } = await response.json();

    // Update UI with fee amount
    document.getElementById('paypal-fee').textContent = fee.toFixed(2) + ' €';
    document.getElementById('paypal-fee-row').classList.remove('hidden');
}
```

## Request/Response Models

### EvPayPalCreateOrderRequest

```csharp
public class EvPayPalCreateOrderRequest
{
    public String ReturnUrl { get; set; } = String.Empty;
    public String CancelUrl { get; set; } = String.Empty;
}
```

### EvPayPalCaptureOrderRequest

```csharp
public class EvPayPalCaptureOrderRequest
{
    [Required]
    public EvPayPalBillingAddressModel BillingAddress { get; set; } = new();
    public String ShippingMethod { get; set; } = "standard";
    public String? Notes { get; set; }
}
```

### EvPayPalCreateOrderResponse

```csharp
public class EvPayPalCreateOrderResponse
{
    public String Id { get; set; } = String.Empty;
    public String Status { get; set; } = String.Empty;
    public String ApprovalUrl { get; set; } = String.Empty;
}
```

### EvPayPalCaptureOrderResponse

```csharp
public class EvPayPalCaptureOrderResponse
{
    public String Id { get; set; } = String.Empty;
    public String Status { get; set; } = String.Empty;
    public String OrderId { get; set; } = String.Empty;
}
```
