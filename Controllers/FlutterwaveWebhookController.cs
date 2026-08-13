using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eshop.Modules.Flutterwave.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Eshop.Modules.Flutterwave.Controllers;

[ApiController]
[Route("api/webhooks/flutterwave")]
/// <summary>
/// Reference Webhook Controller for receiving Flutterwave payment notifications.
/// </summary>
public class FlutterwaveWebhookController : ControllerBase
{
    private readonly FlutterwaveWebhookVerifier _verifier;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<FlutterwaveWebhookController> _logger;

    public FlutterwaveWebhookController(
        FlutterwaveWebhookVerifier verifier,
        IPaymentGateway paymentGateway,
        ILogger<FlutterwaveWebhookController> logger)
    {
        _verifier = verifier;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    /// <summary>
    /// Receives and processes a webhook payload from Flutterwave.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        var signature = Request.Headers["verif-hash"].ToString();
        if (!_verifier.VerifySignature(signature))
        {
            _logger.LogWarning("Invalid Flutterwave webhook signature.");
            return Unauthorized("Invalid signature.");
        }

        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);

        var notification = _verifier.ParseNotification(json);
        if (notification == null)
        {
            return BadRequest("Invalid payload.");
        }

        _logger.LogInformation("Received Flutterwave payment webhook: {EventId}, TransactionId={TxId}, Status={Type}", 
            notification.ProviderEventId, notification.ProviderTransactionId, notification.Type);

        if (notification.Type == PaymentNotificationType.Successful)
        {
            // Verify payment directly with Flutterwave API to ensure authenticity
            var verifyResult = await _paymentGateway.VerifyPaymentAsync(notification.ProviderTransactionId, cancellationToken);
            if (verifyResult.Success && verifyResult.Data?.IsSuccessful == true)
            {
                _logger.LogInformation("Payment verified successfully for Merchant Ref: {MerchantRef}", verifyResult.Data.MerchantReference);
                // TODO: Update order status in your application database here
            }
        }

        return Ok();
    }
}
