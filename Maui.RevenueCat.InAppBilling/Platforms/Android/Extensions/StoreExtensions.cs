using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class StoreExtensions
{
    internal static StoreId ToStoreId(this Store store)
    {
        if (store == Store.Promotional) return StoreId.Promotional;
        if (store == Store.PlayStore) return StoreId.PlayStore;
        if (store == Store.AppStore) return StoreId.AppStore;
        if (store == Store.Amazon) return StoreId.Amazon;
        if (store == Store.Stripe) return StoreId.Stripe;
        if (store == Store.MacAppStore) return StoreId.MacAppStore;
        return StoreId.UnknownStore;
    }
}
