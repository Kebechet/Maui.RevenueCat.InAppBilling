using Com.Revenuecat.Purchases.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class StoreProductExtensions
{
    internal static long? GetComparablePrice(this IStoreProduct storeProduct, bool isSandbox)
    {
        return SubscriptionReplacementRules.GetComparablePrice(
            storeProduct.Price.AmountMicros, storeProduct.Period?.ValueInMonths, isSandbox);
    }

    // A purchase made from a Package buys the product's default option.
    internal static bool IsDefaultOptionPrepaid(this IStoreProduct storeProduct)
    {
        return storeProduct.DefaultOption?.IsPrepaid == true;
    }
}
