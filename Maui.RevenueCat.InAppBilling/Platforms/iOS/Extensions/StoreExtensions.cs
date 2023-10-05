using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
internal static class StoreExtensions
{
    internal static StoreId ToStoreId(this RCStore store)
    {
        switch (store)
        {
            case RCStore.AppStore:
                return StoreId.AppStore;
            case RCStore.MacAppStore:
                return StoreId.MacAppStore;
            case RCStore.PlayStore:
                return StoreId.PlayStore;
            case RCStore.Amazon:
                return StoreId.Amazon;
            case RCStore.Promotional:
                return StoreId.Promotional;
            case RCStore.Stripe:
                return StoreId.Stripe;
            default:
                return StoreId.UnknownStore;
        }
    }
}
