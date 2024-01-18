namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record PackageDto
{
    public string OfferingIdentifier { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public ProductDto Product { get; init; } = new();
}
