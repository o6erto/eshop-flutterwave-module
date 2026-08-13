using System;

namespace Eshop.Modules.Flutterwave.Abstractions;

public sealed record CreatePaymentSessionRequest(
    string OrderId,
    string CustomerEmail,
    string CustomerName,
    decimal Amount,
    string Currency,
    string IdempotencyKey,
    string RedirectUrl);

public sealed record CreatePaymentSessionResult(
    string Provider,
    string ProviderReference,
    Uri CheckoutUrl,
    DateTime ReservationExpiresAtUtc);

public sealed record VerifiedPaymentResult(
    string ProviderTransactionId,
    string MerchantReference,
    decimal Amount,
    string Currency,
    bool IsSuccessful);

public sealed record RefundPaymentRequest(
    string ProviderTransactionId,
    decimal Amount,
    string IdempotencyKey);

public sealed record RefundResult(
    bool IsSuccessful,
    string ProviderRefundId);

public enum PaymentNotificationType
{
    Successful,
    Failed,
    Refunded
}

public sealed record PaymentNotification(
    string ProviderEventId,
    string ProviderTransactionId,
    string MerchantReference,
    PaymentNotificationType Type);
