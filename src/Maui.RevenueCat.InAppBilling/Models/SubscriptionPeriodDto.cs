using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Cross-platform billing period of a subscription product. For a yearly plan,
/// <c>Value=1, Unit=Year</c>; for a weekly plan, <c>Value=1, Unit=Week</c>.
/// Non-subscription products (consumables / lifetime) have <see cref="ProductDto.SubscriptionPeriod"/>
/// set to <c>null</c>.
/// </summary>
public sealed record SubscriptionPeriodDto
{
    public int Value { get; init; }
    public SubscriptionUnit Unit { get; init; } = SubscriptionUnit.Unknown;
}
