using System.ComponentModel.DataAnnotations;

namespace Eshop.Modules.Flutterwave;

public class FlutterwaveOptions
{
    [Required]
    public string BaseUrl { get; set; } = "https://api.flutterwave.com";

    [Required]
    public string PublicKey { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string WebhookSecret { get; set; } = string.Empty;

    [Required]
    public string RedirectUrl { get; set; } = string.Empty;
}
