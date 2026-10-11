using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class CustomerInfoFetchPolicyExtensions
{
    internal static RCCacheFetchPolicy ToRCCacheFetchPolicy(this CustomerInfoFetchPolicy fetchPolicy)
    {
        return fetchPolicy switch
        {
            CustomerInfoFetchPolicy.CachedOrFetched => RCCacheFetchPolicy.CachedOrFetched,
            CustomerInfoFetchPolicy.FetchCurrent => RCCacheFetchPolicy.FetchCurrent,
            CustomerInfoFetchPolicy.NotStaleCachedOrFetched => RCCacheFetchPolicy.NotStaleCachedOrFetched,
            CustomerInfoFetchPolicy.FromCacheOnly => RCCacheFetchPolicy.FromCacheOnly,
            _ => throw new ArgumentOutOfRangeException(nameof(fetchPolicy), fetchPolicy, null)
        };
    }
}
