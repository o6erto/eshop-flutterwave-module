using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eshop.Modules.Flutterwave.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eshop.Modules.Flutterwave;

public class FlutterwaveWebhookVerifier
{
    private readonly FlutterwaveOptions _options;
    private readonly ILogger<FlutterwaveWebhookVerifier> _logger;

    public FlutterwaveWebhookVerifier(IOptions<FlutterwaveOptions> options, ILogger<FlutterwaveWebhookVerifier> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool VerifySignature(string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        return signatureHeader.Equals(_options.WebhookSecret, StringComparison.Ordinal);
    }

    public PaymentNotification? ParseNotification(string payload)
    {
        try
        {
            var data = JsonSerializer.Deserialize<FlutterwaveWebhookPayload>(payload);
            if (data == null)
                return null;

            PaymentNotificationType type = PaymentNotificationType.Failed;
            if (data.Event == "charge.completed" && data.Data.Status == "successful")
            {
                type = PaymentNotificationType.Successful;
            }
            else if (data.Event == "charge.completed" && data.Data.Status == "failed")
            {
                type = PaymentNotificationType.Failed;
            }

            return new PaymentNotification(
                ProviderEventId: data.Data.Id.ToString(),
                ProviderTransactionId: data.Data.Id.ToString(),
                MerchantReference: data.Data.TxRef,
                Type: type
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Flutterwave webhook payload.");
            return null;
        }
    }
}

public class FlutterwaveWebhookPayload
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public FlutterwaveWebhookData Data { get; set; } = new();
}

public class FlutterwaveWebhookData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("tx_ref")]
    public string TxRef { get; set; } = string.Empty;

    [JsonPropertyName("flw_ref")]
    public string FlwRef { get; set; } = string.Empty;

    [JsonPropertyName("device_fingerprint")]
    public string DeviceFingerprint { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("charged_amount")]
    public decimal ChargedAmount { get; set; }

    [JsonPropertyName("app_fee")]
    public decimal AppFee { get; set; }

    [JsonPropertyName("merchant_fee")]
    public decimal MerchantFee { get; set; }

    [JsonPropertyName("processor_response")]
    public string ProcessorResponse { get; set; } = string.Empty;

    [JsonPropertyName("auth_model")]
    public string AuthModel { get; set; } = string.Empty;

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("narration")]
    public string Narration { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("payment_type")]
    public string PaymentType { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public int AccountId { get; set; }
}
