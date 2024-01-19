using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
internal static class StoreExtensions
{
    internal static StoreType ToStoreType(this RCStore store)
    {
        switch (store)
        {
            case RCStore.AppStore:
                return StoreType.AppStore;
            case RCStore.MacAppStore:
                return StoreType.MacAppStore;
            case RCStore.PlayStore:
                return StoreType.PlayStore;
            case RCStore.Amazon:
                return StoreType.Amazon;
            case RCStore.Promotional:
                return StoreType.Promotional;
            case RCStore.Stripe:
                return StoreType.Stripe;
            default:
                return StoreType.UnknownStore;
        }
    }
}
