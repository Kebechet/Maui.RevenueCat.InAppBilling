namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record PricingDto
{
    public string CurrencyCode { get; init; } = string.Empty;

    public decimal Price { get; init; }
    public long PriceMicros { get; init; }
    public string PriceLocalized { get; init; } = string.Empty;
}
