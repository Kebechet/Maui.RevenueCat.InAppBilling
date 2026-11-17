namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record ProductDto
{
    public PricingDto Pricing { get; init; } = new();
    public string Sku { get; init; } = string.Empty;
    public SubscriptionPeriodDto? SubscriptionPeriod { get; init; }

    /// <summary>
    /// The free trial a purchase of this product starts for the current user: <c>Value=2, Unit=Week</c> for a
    /// two-week trial. <c>null</c> when the product has no free trial or the user can no longer get it.
    /// Google Play leaves out the offers a user is not eligible for; on Apple platforms the wrapper asks RevenueCat
    /// and keeps the trial only when the answer is eligible, so an unknown answer shows no trial.
    /// </summary>
    public SubscriptionPeriodDto? FreeTrialPeriod { get; init; }
}
