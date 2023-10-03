namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record OfferingDto
{
    public string Identifier { get; init; } = string.Empty;
    public List<PackageDto> AvailablePackages { get; init; } = new();
    public bool Current { get; init; } = false;
}
