# Eshop Flutterwave Payment Integration Module

A standalone, production-ready .NET 8 / .NET 10 C# module for integrating **Flutterwave Payment Gateway** into ASP.NET Core web applications.

Features out-of-the-box support for **Credit/Debit Cards** and **African Mobile Money** (MTN, Airtel, Zamtel, etc.).

---

## Features

- 💳 **Hosted Checkout Session Creation** (`POST /v3/payments`)
- 📱 **Zambian & African Mobile Money Support** (MTN, Airtel, Zamtel)
- 🔒 **Webhook Signature Verification** (`verif-hash` header validation)
- 🔍 **Server-to-Server Transaction Verification** (`GET /v3/transactions/{id}/verify`)
- 🛡️ **Polly-Ready / Options-Validated DI Extensions**

---

## Getting Started

### 1. Register in `Program.cs`

Add the module to your `IServiceCollection`:

```csharp
using Eshop.Modules.Flutterwave;

var builder = WebApplication.CreateBuilder(args);

// Register Flutterwave API Client & Services
builder.Services.AddFlutterwaveModule(builder.Configuration);
```

### 2. Configure `appsettings.json`

Add the `Flutterwave` settings block to your `appsettings.json`:

```json
{
  "Flutterwave": {
    "BaseUrl": "https://api.flutterwave.com",
    "PublicKey": "FLWPUBK_TEST-xxxxxxxxxxxxxxxxxxxxxxxx-X",
    "SecretKey": "FLWSECK_TEST-xxxxxxxxxxxxxxxxxxxxxxxx-X",
    "WebhookSecret": "your_custom_webhook_secret_hash",
    "RedirectUrl": "http://localhost:3000/checkout/success"
  }
}
```

---

## Usage Examples

### Creating a Payment Session

```csharp
public class CheckoutService
{
    private readonly IPaymentGateway _paymentGateway;

    public CheckoutService(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    public async Task<string> ProcessCheckoutAsync(string orderId, decimal amount, string email, string name)
    {
        var request = new CreatePaymentSessionRequest(
            OrderId: orderId,
            CustomerEmail: email,
            CustomerName: name,
            Amount: amount,
            Currency: "ZMW",
            IdempotencyKey: Guid.NewGuid().ToString(),
            RedirectUrl: "http://localhost:3000/checkout/success"
        );

        var result = await _paymentGateway.CreatePaymentSessionAsync(request, CancellationToken.None);

        if (!result.Success || result.Data == null)
        {
            throw new Exception(result.ErrorMessage);
        }

        // Redirect user to Flutterwave hosted checkout link
        return result.Data.CheckoutUrl.ToString();
    }
}
```

### Receiving & Verifying Webhooks

Register the provided `FlutterwaveWebhookController` or call `FlutterwaveWebhookVerifier` directly:

```csharp
[HttpPost("api/webhooks/flutterwave")]
public async Task<IActionResult> HandleWebhook()
{
    var signature = Request.Headers["verif-hash"].ToString();
    if (!_verifier.VerifySignature(signature))
    {
        return Unauthorized("Invalid signature.");
    }

    using var reader = new StreamReader(Request.Body);
    var json = await reader.ReadToEndAsync();
    var notification = _verifier.ParseNotification(json);

    if (notification?.Type == PaymentNotificationType.Successful)
    {
        // Double-check with Flutterwave verification API before fulfilling order
        var verifyResult = await _paymentGateway.VerifyPaymentAsync(notification.ProviderTransactionId, CancellationToken.None);
        if (verifyResult.Success && verifyResult.Data?.IsSuccessful == true)
        {
            // Complete order in database
        }
    }

    return Ok();
}
```

---

## License
MIT License. Free for commercial and non-commercial use.
