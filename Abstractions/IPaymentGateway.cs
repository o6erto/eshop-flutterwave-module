using System.Threading;
using System.Threading.Tasks;

namespace Eshop.Modules.Flutterwave.Abstractions;

public interface IPaymentGateway
{
    Task<Result<CreatePaymentSessionResult>> CreatePaymentSessionAsync(
        CreatePaymentSessionRequest request,
        CancellationToken cancellationToken);

    Task<Result<VerifiedPaymentResult>> VerifyPaymentAsync(
        string providerTransactionId,
        CancellationToken cancellationToken);

    Task<Result<RefundResult>> RefundAsync(
        RefundPaymentRequest request,
        CancellationToken cancellationToken);
}
