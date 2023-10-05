using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Models;
public sealed record EntitlementInfoDto
{
    public required DateTime? BillingIssueDetectedAt { get; init; }
    public required DateTime? ExpirationDate { get; init; }
    public required string Identifier { get; init; }
    public required bool IsActive { get; init; }
    public required bool IsSandbox { get; init; }
    public required DateTime? LatestPurchaseDate { get; init; }
    public required DateTime? OriginalPurchaseDate { get; init; }
    public required OwnershipId OwnershipType { get; init; }
    public required PeriodId PeriodType { get; init; }
    public required string ProductIdentifier { get; init; }
    public required string ProductPlanIdentifier { get; init; }
    public required StoreId Store { get; init; }
    public required DateTime? UnsubscribeDetectedAt { get; init; }
    public required bool WillRenew { get; init; }
}
