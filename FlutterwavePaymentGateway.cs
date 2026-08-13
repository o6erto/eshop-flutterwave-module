using System;
using System.Threading;
using System.Threading.Tasks;
using Eshop.Modules.Flutterwave.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eshop.Modules.Flutterwave;

/// <summary>
/// Implements the IPaymentGateway interface for Flutterwave.
/// This adapter isolates core applications from Flutterwave-specific API contracts and models.
/// </summary>
public sealed class FlutterwavePaymentGateway : IPaymentGateway
{
    private readonly FlutterwaveApiClient _apiClient;
    private readonly FlutterwaveOptions _options;
    private readonly ILogger<FlutterwavePaymentGateway> _logger;

    public FlutterwavePaymentGateway(
        FlutterwaveApiClient apiClient, 
        IOptions<FlutterwaveOptions> options, 
        ILogger<FlutterwavePaymentGateway> logger)
    {
        _apiClient = apiClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<CreatePaymentSessionResult>> CreatePaymentSessionAsync(CreatePaymentSessionRequest request, CancellationToken cancellationToken)
    {
        // Fallback to configured FlutterwaveOptions.RedirectUrl if request URL is empty or placeholder
        var redirectUrl = string.IsNullOrWhiteSpace(request.RedirectUrl) || request.RedirectUrl.Contains("your-frontend.com")
            ? (string.IsNullOrWhiteSpace(_options.RedirectUrl) 
                ? $"http://localhost:3000/checkout/success?order={request.OrderId}"
                : $"{_options.RedirectUrl.TrimEnd('/')}?order={request.OrderId}")
            : request.RedirectUrl;

        // TxRef acts as the idempotency key and merchant reference for Flutterwave. 
        // It encapsulates the OrderId to ensure the transaction can be traced back 
        // to the correct order during webhook processing.
        var apiRequest = new FlutterwavePaymentRequest
        {
            TxRef = $"payment-{request.OrderId}-{request.IdempotencyKey}",
            Amount = request.Amount,
            Currency = request.Currency,
            RedirectUrl = redirectUrl,
            Customer = new FlutterwaveCustomer
            {
                Email = request.CustomerEmail,
                Name = request.CustomerName
            }
        };

        var response = await _apiClient.CreatePaymentSessionAsync(apiRequest, cancellationToken);

        if (!response.Success || response.Data == null)
        {
            return Result<CreatePaymentSessionResult>.Failed(ResultErrorCode.InternalError, response.ErrorMessage ?? "Flutterwave API returned failure.");
        }

        var result = new CreatePaymentSessionResult(
            Provider: "Flutterwave",
            ProviderReference: apiRequest.TxRef,
            CheckoutUrl: new Uri(response.Data.Data.Link),
            ReservationExpiresAtUtc: DateTime.UtcNow.AddMinutes(15)
        );

        return Result<CreatePaymentSessionResult>.SuccessResult(result);
    }

    public async Task<Result<VerifiedPaymentResult>> VerifyPaymentAsync(string providerTransactionId, CancellationToken cancellationToken)
    {
        var response = await _apiClient.VerifyTransactionAsync(providerTransactionId, cancellationToken);

        if (!response.Success || response.Data == null)
        {
            return Result<VerifiedPaymentResult>.Failed(ResultErrorCode.InternalError, response.ErrorMessage ?? "Flutterwave verification failed.");
        }

        var data = response.Data.Data;
        var result = new VerifiedPaymentResult(
            ProviderTransactionId: data.Id.ToString(),
            MerchantReference: data.TxRef,
            Amount: data.Amount,
            Currency: data.Currency,
            IsSuccessful: data.Status.Equals("successful", StringComparison.OrdinalIgnoreCase)
        );

        return Result<VerifiedPaymentResult>.SuccessResult(result);
    }

    public Task<Result<RefundResult>> RefundAsync(RefundPaymentRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Refund functionality is not yet implemented in the Flutterwave module.");
    }
}
