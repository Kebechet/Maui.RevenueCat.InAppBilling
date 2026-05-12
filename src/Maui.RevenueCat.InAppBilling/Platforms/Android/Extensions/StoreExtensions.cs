using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class StoreExtensions
{
    internal static StoreType ToStoreType(this Store store)
    {
        if (store == Store.Promotional) return StoreType.Promotional;
        if (store == Store.PlayStore) return StoreType.PlayStore;
        if (store == Store.AppStore) return StoreType.AppStore;
        if (store == Store.Amazon) return StoreType.Amazon;
        if (store == Store.Stripe) return StoreType.Stripe;
        if (store == Store.MacAppStore) return StoreType.MacAppStore;
        if (store == Store.RcBilling) return StoreType.RcBilling;
        if (store == Store.External) return StoreType.External;
        if (store == Store.Paddle) return StoreType.Paddle;
        if (store == Store.TestStore) return StoreType.TestStore;
        if (store == Store.Galaxy) return StoreType.Galaxy;
        return StoreType.UnknownStore;
    }
}
