using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Eshop.Modules.Flutterwave.Abstractions;

namespace Eshop.Modules.Flutterwave;

public static class DependencyInjection
{
    public static IHttpClientBuilder AddFlutterwaveModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FlutterwaveOptions>()
            .Bind(configuration.GetSection("Flutterwave"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var builder = services.AddHttpClient<FlutterwaveApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FlutterwaveOptions>>().Value;
            client.BaseAddress = new System.Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.SecretKey);
        });

        services.AddScoped<IPaymentGateway, FlutterwavePaymentGateway>();
        services.AddScoped<FlutterwaveWebhookVerifier>();

        return builder;
    }
}
