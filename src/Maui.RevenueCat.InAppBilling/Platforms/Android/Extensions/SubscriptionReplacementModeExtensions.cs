using Com.Revenuecat.Purchases.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class SubscriptionReplacementModeExtensions
{
    internal static StoreReplacementMode ToStoreReplacementMode(this SubscriptionReplacementMode replacementMode)
    {
        return replacementMode switch
        {
            SubscriptionReplacementMode.ChargeProratedPrice => StoreReplacementMode.ChargeProratedPrice,
            SubscriptionReplacementMode.Deferred => StoreReplacementMode.Deferred,
            SubscriptionReplacementMode.ChargeFullPrice => StoreReplacementMode.ChargeFullPrice,
            _ => throw new ArgumentOutOfRangeException(nameof(replacementMode), replacementMode, null)
        };
    }
}
