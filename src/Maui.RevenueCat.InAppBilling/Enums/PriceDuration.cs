namespace Maui.RevenueCat.InAppBilling.Enums;

/// <summary>
/// Target period a package price is normalized to when displaying a "price per X" figure
/// in the UI (e.g. <c>$1.99/month</c> for a yearly subscription).
/// </summary>
public enum PriceDuration
{
    Daily,
    Weekly,
    Monthly,
    Yearly,
}
