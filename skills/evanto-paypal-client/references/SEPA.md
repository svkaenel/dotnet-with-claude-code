# SEPA Direct Debit Integration Reference

Complete implementation for SEPA Direct Debit as an alternative subscription payment method alongside PayPal.

## Backend: Subscription Controller

The `SubscriptionController` handles SEPA-specific subscription operations, separate from the `PayPalController` which handles PayPal subscriptions.

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController(
    ISubscriptionService subscriptionService,
    ISepaValidationService sepaValidationService,
    ILogger<SubscriptionController> logger) : ControllerBase
{
    private readonly ISubscriptionService          mSubscriptionService   = subscriptionService;
    private readonly ISepaValidationService        mSepaValidationService = sepaValidationService;
    private readonly ILogger<SubscriptionController> mLogger                = logger;

    /// <summary>   Approves a SEPA direct debit subscription. </summary>
    [HttpPost("approve-sepa")]
    public async Task<ActionResult<SubscriptionApprovalResponse>> ApproveSepaSubscription(
        [FromBody] SepaSubscriptionRequest request)
    {
        if (!ModelState.IsValid || request == null)
        {
            return BadRequest(new SubscriptionApprovalResponse
            {
                Success = false,
                Error   = "Invalid request"
            });
        }

        try
        {   // 1. Pre-validate IBAN
            var (ibanValid, ibanError, countryCode) = mSepaValidationService.ValidateIban(request.Iban);
            if (!ibanValid)
            {
                return BadRequest(new SubscriptionApprovalResponse
                {
                    Success = false,
                    Error   = ibanError
                });
            }

            // 2. Verify SEPA country availability
            if (!mSepaValidationService.IsSepaCountry(request.BillingAddress.Country))
            {
                return BadRequest(new SubscriptionApprovalResponse
                {
                    Success = false,
                    Error   = "SEPA direct debit is not available in your country"
                });
            }

            // 3. Process subscription (includes user creation, email sending)
            var baseUrl         = $"{Request.Scheme}://{Request.Host}";
            var verificationUrl = $"{baseUrl}/verify-email";
            var response        = await mSubscriptionService.ProcessSepaSubscriptionAsync(
                request, verificationUrl, baseUrl);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to approve SEPA subscription for {Email}",
                request.BillingAddress.Email);
            return StatusCode(500, new SubscriptionApprovalResponse
            {
                Success = false,
                Error   = "Failed to process subscription"
            });
        }
    }

    /// <summary>   Validates an IBAN and returns country information. </summary>
    [HttpGet("validate-iban")]
    public ActionResult ValidateIban([FromQuery] String iban)
    {
        if (String.IsNullOrWhiteSpace(iban))
        {
            return BadRequest(new { valid = false, error = "IBAN is required" });
        }

        var (isValid, error, countryCode) = mSepaValidationService.ValidateIban(iban);

        if (!isValid)
        {
            return Ok(new { valid = false, error });
        }

        var isSepaCountry = mSepaValidationService.IsSepaCountry(countryCode!);

        return Ok(new
        {
            valid         = true,
            countryCode,
            isSepaCountry,
            maskedIban    = mSepaValidationService.MaskIban(iban)
        });
    }

    /// <summary>   Gets the list of SEPA zone countries. </summary>
    [HttpGet("sepa-countries")]
    public ActionResult GetSepaCountries([FromQuery] String culture = "de")
    {
        var countries = mSepaValidationService.GetSepaCountries(culture)
            .Select(c => new { code = c.Code, name = c.Name })
            .ToList();

        return Ok(countries);
    }
}
```

## SEPA Request Model

```csharp
public class SepaSubscriptionRequest
{
    public String PlanId { get; set; } = String.Empty;
    public BillingAddressRequest BillingAddress { get; set; } = new();
    public String? Password { get; set; }
    public String AccountHolderName { get; set; } = String.Empty;
    public String Iban { get; set; } = String.Empty;
    public String? Bic { get; set; }
    public Boolean MandateConsent { get; set; }
}
```

## Frontend: SEPA Payment Method on Subscription Checkout

The subscription checkout page offers both PayPal and SEPA as payment methods. The SEPA section is shown/hidden based on the selected payment method and country availability.

### Payment Method Selection

```html
<!-- Payment Method Selection -->
<div class="space-y-3 mb-6">
    <label class="flex items-center p-4 border rounded-lg cursor-pointer payment-method-option"
           data-method="paypal">
        <input type="radio" name="SelectedPaymentMethod" value="paypal" checked />
        <i class="ri-paypal-fill text-2xl text-[#003087] mx-3"></i>
        <div>
            <div class="font-medium">PayPal</div>
            <div class="text-sm text-gray-600">Secure payment via PayPal</div>
        </div>
    </label>

    <label id="sepa-option"
           class="flex items-center p-4 border rounded-lg cursor-pointer payment-method-option"
           data-method="sepa">
        <input type="radio" name="SelectedPaymentMethod" value="sepa" />
        <i class="ri-bank-line text-2xl text-gray-700 mx-3"></i>
        <div>
            <div class="font-medium">SEPA Direct Debit</div>
            <div class="text-sm text-gray-600">Monthly debit from your bank account</div>
        </div>
    </label>
</div>
```

### SEPA Form Fields

```html
<!-- SEPA Section (hidden by default, shown when SEPA selected) -->
<div id="sepa-section" class="hidden">
    <div class="space-y-4">
        <div>
            <label for="SepaForm_AccountHolderName">Account Holder *</label>
            <input type="text" id="SepaForm_AccountHolderName"
                   placeholder="Max Mustermann" />
        </div>

        <div>
            <label for="SepaForm_Iban">IBAN *</label>
            <input type="text" id="SepaForm_Iban" class="font-mono"
                   placeholder="DE89 3704 0044 0532 0130 00" />
            <p id="iban-validation-message" class="text-xs mt-1 hidden"></p>
        </div>

        <div>
            <label for="SepaForm_Bic">BIC (optional)</label>
            <input type="text" id="SepaForm_Bic" class="font-mono"
                   placeholder="COBADEFFXXX" />
        </div>

        <!-- SEPA Mandate Consent -->
        <div class="p-4 bg-gray-50 border rounded-lg">
            <h4 class="font-medium mb-2">Direct Debit Mandate</h4>
            <p class="text-sm text-gray-600 mb-3">
                I authorize the creditor to collect payments from my account
                by direct debit. I also instruct my bank to honour the direct
                debits drawn on my account.
            </p>
            <label class="flex items-start gap-2">
                <input type="checkbox" id="SepaForm_MandateConsent" />
                <span class="text-sm">I agree to the SEPA direct debit mandate</span>
            </label>
        </div>

        <!-- SEPA Submit Button -->
        <button type="button" id="sepa-submit-button"
                class="w-full bg-primary text-white py-3 px-6 rounded-button font-medium">
            Subscribe with SEPA Direct Debit
        </button>
    </div>
</div>

<!-- SEPA Not Available Message (shown for non-SEPA countries) -->
<div id="sepa-not-available" class="hidden p-4 bg-yellow-100 border border-yellow-300 text-yellow-700 rounded-lg">
    SEPA direct debit is not available for your country.
</div>
```

### SEPA JavaScript Integration

```javascript
document.addEventListener('DOMContentLoaded', function() {
    // SEPA zone country codes (loaded from server or inline)
    const sepaCountries = ["AT","BE","BG","HR","CY","CZ","DK","EE","FI","FR",
        "DE","GR","HU","IS","IE","IT","LV","LI","LT","LU","MT","MC","NL","NO",
        "PL","PT","RO","SM","SK","SI","ES","SE","CH","GB","AD","VA","GI","GG",
        "JE","IM","PM","MF","BL","MQ","GP","GF","RE","YT"];

    let selectedPaymentMethod = 'paypal';
    const countrySelect = document.getElementById('BillingAddress_Country');
    const sepaOption = document.getElementById('sepa-option');
    const ibanInput = document.getElementById('SepaForm_Iban');

    // Payment method switching
    document.querySelectorAll('input[name="SelectedPaymentMethod"]').forEach(radio => {
        radio.addEventListener('change', function() {
            selectedPaymentMethod = this.value;
            updatePaymentMethodUI();
        });
    });

    function updatePaymentMethodUI() {
        const paypalSection = document.getElementById('paypal-section');
        const sepaSection = document.getElementById('sepa-section');

        if (selectedPaymentMethod === 'paypal') {
            paypalSection.classList.remove('hidden');
            sepaSection.classList.add('hidden');
        } else {
            paypalSection.classList.add('hidden');
            sepaSection.classList.remove('hidden');
        }
    }

    // Check SEPA availability based on country
    function checkSepaAvailability() {
        const country = countrySelect.value;
        const isAvailable = sepaCountries.includes(country);

        if (isAvailable) {
            sepaOption.classList.remove('opacity-50', 'pointer-events-none');
            sepaOption.querySelector('input').disabled = false;
        } else {
            sepaOption.classList.add('opacity-50', 'pointer-events-none');
            sepaOption.querySelector('input').disabled = true;

            // If SEPA was selected, switch back to PayPal
            if (selectedPaymentMethod === 'sepa') {
                document.querySelector('input[name="SelectedPaymentMethod"][value="paypal"]').checked = true;
                selectedPaymentMethod = 'paypal';
                updatePaymentMethodUI();
            }
        }
    }

    countrySelect.addEventListener('change', checkSepaAvailability);

    // Real-time IBAN validation
    let ibanValidationTimeout;
    ibanInput.addEventListener('input', function() {
        clearTimeout(ibanValidationTimeout);
        ibanValidationTimeout = setTimeout(validateIban, 500);
    });

    async function validateIban() {
        const iban = ibanInput.value.replace(/\s/g, '');
        if (!iban || iban.length < 15) return;

        try {
            const response = await fetch(
                `/api/subscription/validate-iban?iban=${encodeURIComponent(iban)}`);
            const result = await response.json();
            const msg = document.getElementById('iban-validation-message');

            if (result.valid) {
                msg.textContent = `Valid: ${result.maskedIban}`;
                msg.classList.remove('hidden', 'text-red-500');
                msg.classList.add('text-green-600');
                ibanInput.classList.remove('border-red-500');
                ibanInput.classList.add('border-green-500');

                if (!result.isSepaCountry) {
                    msg.textContent = 'IBAN is valid, but SEPA is not available for this country.';
                    msg.classList.remove('text-green-600');
                    msg.classList.add('text-yellow-600');
                }
            } else {
                msg.textContent = result.error || 'Invalid IBAN';
                msg.classList.remove('hidden', 'text-green-600');
                msg.classList.add('text-red-500');
                ibanInput.classList.remove('border-green-500');
                ibanInput.classList.add('border-red-500');
            }
        } catch (error) {
            console.error('IBAN validation error:', error);
        }
    }

    // SEPA Submit handler
    document.getElementById('sepa-submit-button').addEventListener('click', async function() {
        // Validate billing address form
        if (!validateForm()) return;

        // Validate SEPA-specific fields
        const accountHolder = document.getElementById('SepaForm_AccountHolderName');
        const iban = document.getElementById('SepaForm_Iban');
        const mandateConsent = document.getElementById('SepaForm_MandateConsent');

        if (!accountHolder.value.trim() || !iban.value.trim() || !mandateConsent.checked) {
            // Show validation errors
            return;
        }

        this.disabled = true;
        this.textContent = 'Processing...';

        try {
            const formData = getFormData(); // Reuse billing address helper
            const response = await fetch('/api/subscription/approve-sepa', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    planId: formData.planId,
                    billingAddress: {
                        firstName:  formData.firstName,
                        lastName:   formData.lastName,
                        email:      formData.email,
                        street:     formData.street,
                        postalCode: formData.postalCode,
                        city:       formData.city,
                        country:    formData.country
                    },
                    password: formData.password,
                    accountHolderName: accountHolder.value,
                    iban: iban.value.replace(/\s/g, ''),
                    bic: document.getElementById('SepaForm_Bic').value || null,
                    mandateConsent: mandateConsent.checked
                })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Failed to process subscription');
            }

            const result = await response.json();
            window.location.href = `/subscription/confirmation?subscriptionId=${result.payPalSubscriptionId}`;

        } catch (error) {
            console.error('SEPA subscription error:', error);
            // Show error to user
        } finally {
            this.disabled = false;
            this.textContent = 'Subscribe with SEPA Direct Debit';
        }
    });

    // Initialize
    checkSepaAvailability();
    updatePaymentMethodUI();
});
```

## SEPA Validation Service Interface

```csharp
public interface ISepaValidationService
{
    /// <summary>   Validates an IBAN. Returns (isValid, errorMessage, countryCode). </summary>
    (Boolean IsValid, String? Error, String? CountryCode) ValidateIban(String iban);

    /// <summary>   Checks if a country code is in the SEPA zone. </summary>
    Boolean IsSepaCountry(String countryCode);

    /// <summary>   Masks an IBAN for display (e.g., "DE89 **** **** **** **** 00"). </summary>
    String MaskIban(String iban);

    /// <summary>   Gets the list of SEPA zone countries. </summary>
    IReadOnlyList<SepaCountry> GetSepaCountries(String culture = "de");
}
```

## SEPA Data Storage

When a SEPA subscription is created, the following data is stored:

| Field | Description | Example |
|---|---|---|
| `sepa_mandate_reference` | Generated mandate ID | `MANDATE-20260204-a1b2c3d4` |
| `sepa_mandate_date` | Mandate creation date | `2026-02-04` |
| `sepa_iban` | Full IBAN (backoffice only) | `DE89370400440532013000` |
| `sepa_iban_masked` | Masked IBAN (customer display) | `DE89 **** **** **** **** 00` |
| `sepa_account_holder` | Account holder name | `Max Mustermann` |
| `payment_method` | Payment method identifier | `sepa_direct_debit` |

## SEPA Dependencies

- **IbanNet** - IBAN validation library (validates format, checksum, country)
- **Nager.Country** - Country data for SEPA zone filtering (41 European countries)

## Recommended Project Structure

Create these files in your web project:

- **SEPA Controller**: `Controllers/Api/SubscriptionController.cs`
- **SEPA Validation Service**: `Infrastructure/Sepa/SepaValidationService.cs` (implements `ISepaValidationService`)
- **Subscription Checkout**: `Pages/Subscription/Checkout.cshtml` (or equivalent view with both PayPal and SEPA payment methods)
