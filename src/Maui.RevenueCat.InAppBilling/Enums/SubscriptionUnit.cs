namespace Maui.RevenueCat.InAppBilling.Enums;

/// <summary>
/// Time-unit granularity for a subscription's billing period.
/// Cross-platform equivalent of iOS <c>RCSubscriptionPeriodUnit</c> and Android <c>Period.Unit</c>.
/// </summary>
public enum SubscriptionUnit
{
    Unknown = 0,
    Day,
    Week,
    Month,
    Year,
}
