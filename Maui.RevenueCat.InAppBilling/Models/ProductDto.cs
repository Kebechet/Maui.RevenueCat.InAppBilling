namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record ProductDto
{
    public string Price { get; init; } = string.Empty;
    public string PriceCurrencyCode { get; init; } = string.Empty;
    public long OriginalPriceAmountMicros { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string SubscriptionPeriod { get; init; } = string.Empty;
}
