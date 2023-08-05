namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record CustomerInfoDto
{
    public required List<string> ActiveSubscriptions { get; init; }
    public required List<string> AllPurchasedIdentifiers { get; init; }
    public required List<string> NonConsumablePurchases { get; init; }
    public required DateTime? FirstSeen { get; init; }
    public required DateTime? LatestExpirationDate { get; init; }
    public required string? ManagementURL { get; init; }
}
