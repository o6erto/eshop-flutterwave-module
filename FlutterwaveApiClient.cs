using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Eshop.Modules.Flutterwave.Abstractions;
using Microsoft.Extensions.Logging;

namespace Eshop.Modules.Flutterwave;

public class FlutterwaveApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FlutterwaveApiClient> _logger;

    public FlutterwaveApiClient(HttpClient httpClient, ILogger<FlutterwaveApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<FlutterwavePaymentResponse>> CreatePaymentSessionAsync(FlutterwavePaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/v3/payments", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Flutterwave API error: {Error}", error);
                return Result<FlutterwavePaymentResponse>.Failed(ResultErrorCode.InternalError, "Payment creation failed at provider.");
            }

            var result = await response.Content.ReadFromJsonAsync<FlutterwavePaymentResponse>(cancellationToken: cancellationToken);
            if (result == null || result.Status != "success")
            {
                return Result<FlutterwavePaymentResponse>.Failed(ResultErrorCode.InternalError, "Invalid response from Flutterwave.");
            }

            return Result<FlutterwavePaymentResponse>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception communicating with Flutterwave.");
            return Result<FlutterwavePaymentResponse>.Failed(ResultErrorCode.InternalError, "Payment creation failed.");
        }
    }

    public async Task<Result<FlutterwaveVerifyResponse>> VerifyTransactionAsync(string transactionId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v3/transactions/{transactionId}/verify", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Flutterwave API verification error: {Error}", error);
                return Result<FlutterwaveVerifyResponse>.Failed(ResultErrorCode.InternalError, "Payment verification failed at provider.");
            }

            var result = await response.Content.ReadFromJsonAsync<FlutterwaveVerifyResponse>(cancellationToken: cancellationToken);
            if (result == null || result.Status != "success")
            {
                return Result<FlutterwaveVerifyResponse>.Failed(ResultErrorCode.InternalError, "Invalid response from Flutterwave verification.");
            }

            return Result<FlutterwaveVerifyResponse>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception communicating with Flutterwave during verification.");
            return Result<FlutterwaveVerifyResponse>.Failed(ResultErrorCode.InternalError, "Payment verification failed.");
        }
    }
}

// API Models
public class FlutterwavePaymentRequest
{
    [JsonPropertyName("tx_ref")]
    public string TxRef { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    [JsonPropertyName("redirect_url")]
    public string RedirectUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("customer")]
    public FlutterwaveCustomer Customer { get; set; } = new();
}

public class FlutterwaveCustomer
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class FlutterwavePaymentResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public FlutterwavePaymentResponseData Data { get; set; } = new();
}

public class FlutterwavePaymentResponseData
{
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;
}

public class FlutterwaveVerifyResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public FlutterwaveVerifyResponseData Data { get; set; } = new();
}

public class FlutterwaveVerifyResponseData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("tx_ref")]
    public string TxRef { get; set; } = string.Empty;
    
    [JsonPropertyName("flw_ref")]
    public string FlwRef { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
