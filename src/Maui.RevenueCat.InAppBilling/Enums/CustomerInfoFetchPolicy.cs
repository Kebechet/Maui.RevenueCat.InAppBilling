namespace Maui.RevenueCat.InAppBilling.Enums;

/// <summary>
/// How <see cref="Services.IRevenueCatBilling.GetCustomerInfo"/> treats the customer info the
/// RevenueCat SDK keeps cached on the device. The SDK considers the cache stale after 5 minutes.
/// https://www.revenuecat.com/docs/customers/customer-info
/// </summary>
public enum CustomerInfoFetchPolicy
{
    /// <summary>
    /// Default behavior: returns the cached data if available (even if stale), or fetches
    /// up-to-date data. If the cached data is stale, it initiates a fetch in the background whose
    /// result is not returned by this call.
    /// https://github.com/RevenueCat/purchases-android/blob/main/purchases/src/main/kotlin/com/revenuecat/purchases/CacheFetchPolicy.kt
    /// </summary>
    CachedOrFetched,

    /// <summary>
    /// Always fetch the most up-to-date data. Returns an error if the fetch fails.
    /// https://github.com/RevenueCat/purchases-ios/blob/main/Sources/Networking/Caching/CacheFetchPolicy.swift
    /// </summary>
    FetchCurrent,

    /// <summary>
    /// Returns the cached data if available and not stale, or fetches up-to-date data. If the
    /// cached data is stale and the fetch fails (when offline, for example), an error is returned
    /// instead of the outdated cached data.
    /// https://github.com/RevenueCat/purchases-ios/blob/main/Sources/Networking/Caching/CacheFetchPolicy.swift
    /// </summary>
    NotStaleCachedOrFetched,

    /// <summary>
    /// Returns values from the cache, or an error if not available. It won't initiate a fetch.
    /// https://github.com/RevenueCat/purchases-android/blob/main/purchases/src/main/kotlin/com/revenuecat/purchases/CacheFetchPolicy.kt
    /// </summary>
    FromCacheOnly,
}
