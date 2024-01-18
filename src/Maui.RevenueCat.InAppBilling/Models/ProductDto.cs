namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record ProductDto
{
    public PricingDto Pricing { get; init; } = new();
    public string Sku { get; init; } = string.Empty;
    public string SubscriptionPeriod { get; init; } = string.Empty;
}
