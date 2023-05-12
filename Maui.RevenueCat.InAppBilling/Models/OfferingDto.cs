namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record OfferingDto
{
    public required string Identifier { get; init; }
    public required ProductDto Product { get; init; }
}
