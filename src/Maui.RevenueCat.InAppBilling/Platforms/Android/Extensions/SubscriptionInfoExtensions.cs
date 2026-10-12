using Com.Revenuecat.Purchases;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class SubscriptionInfoExtensions
{
    internal static ActivePlayStoreSubscription ToActivePlayStoreSubscription(this SubscriptionInfo subscriptionInfo)
    {
        return new ActivePlayStoreSubscription(
            subscriptionInfo.ProductIdentifier, subscriptionInfo.ProductPlanIdentifier, subscriptionInfo.IsSandbox);
    }
}
