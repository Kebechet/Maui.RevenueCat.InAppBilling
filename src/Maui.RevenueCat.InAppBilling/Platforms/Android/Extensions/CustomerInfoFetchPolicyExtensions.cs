using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class CustomerInfoFetchPolicyExtensions
{
    internal static CacheFetchPolicy ToRCCacheFetchPolicy(this CustomerInfoFetchPolicy fetchPolicy)
    {
        var revenueCatFetchPolicy = fetchPolicy switch
        {
            CustomerInfoFetchPolicy.CachedOrFetched => CacheFetchPolicy.CachedOrFetched,
            CustomerInfoFetchPolicy.FetchCurrent => CacheFetchPolicy.FetchCurrent,
            CustomerInfoFetchPolicy.NotStaleCachedOrFetched => CacheFetchPolicy.NotStaleCachedOrCurrent,
            CustomerInfoFetchPolicy.FromCacheOnly => CacheFetchPolicy.CacheOnly,
            _ => throw new ArgumentOutOfRangeException(nameof(fetchPolicy), fetchPolicy, null)
        };

        return revenueCatFetchPolicy
            ?? throw new Exception($"Could not convert {nameof(CustomerInfoFetchPolicy)} to {nameof(CacheFetchPolicy)}");
    }
}
