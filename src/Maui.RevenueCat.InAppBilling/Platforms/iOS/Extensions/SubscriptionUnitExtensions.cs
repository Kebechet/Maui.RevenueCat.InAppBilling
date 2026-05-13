using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class SubscriptionUnitExtensions
{
    internal static SubscriptionUnit ToSubscriptionUnit(this RCSubscriptionPeriodUnit unit) => unit switch
    {
        RCSubscriptionPeriodUnit.Day => SubscriptionUnit.Day,
        RCSubscriptionPeriodUnit.Week => SubscriptionUnit.Week,
        RCSubscriptionPeriodUnit.Month => SubscriptionUnit.Month,
        RCSubscriptionPeriodUnit.Year => SubscriptionUnit.Year,
        _ => SubscriptionUnit.Unknown,
    };
}
