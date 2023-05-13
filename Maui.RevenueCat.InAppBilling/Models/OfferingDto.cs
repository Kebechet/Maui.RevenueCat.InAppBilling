namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record OfferingDto
{
    public string Identifier { get; init; } = string.Empty;
    public ProductDto Product { get; init; } = new();
}
