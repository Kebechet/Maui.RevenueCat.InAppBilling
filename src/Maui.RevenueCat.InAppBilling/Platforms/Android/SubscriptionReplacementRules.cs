namespace Maui.RevenueCat.InAppBilling.Platforms.Android;

internal sealed record ActivePlayStoreSubscription(string SubscriptionId, string? BasePlanId, bool IsSandbox);

// Google Play has no subscription groups, so a second subscription product bought without naming the one it
// replaces runs in parallel and the user pays for both. These rules pick the replacement the way RevenueCat's
// own Paywalls do:
// https://github.com/RevenueCat/purchases-android/blob/10.16.1/ui/revenuecatui/src/main/kotlin/com/revenuecat/purchases/ui/revenuecatui/data/ProductChangeCalculator.kt
// The reasoning behind them is in https://github.com/RevenueCat/purchases-android/pull/3062
// Kept free of Android types so DemoHarness.Tests can compile and test it on plain .NET.
internal static class SubscriptionReplacementRules
{
    private const char _basePlanSeparator = ':';

    internal static (string SubscriptionId, string? BasePlanId) ParseProductIdentifier(string productIdentifier)
    {
        var parts = productIdentifier.Split(_basePlanSeparator, 2);

        return (parts[0], parts.Length > 1 ? parts[1] : null);
    }

    internal static ActivePlayStoreSubscription? FindSubscriptionToReplace(
        IReadOnlyCollection<ActivePlayStoreSubscription> activePlayStoreSubscriptions, string newSubscriptionId)
    {
        // RevenueCat's Paywalls replace the first of several active subscriptions. With more than one there is
        // no telling which the new purchase is meant to replace, so it goes ahead without a replacement rather
        // than cancel the wrong one.
        if (activePlayStoreSubscriptions.Count != 1)
        {
            return null;
        }

        var activeSubscription = activePlayStoreSubscriptions.Single();

        // Google Play handles a change between base plans of the same subscription by itself.
        return activeSubscription.SubscriptionId == newSubscriptionId ? null : activeSubscription;
    }

    internal static bool IsProductOf(ActivePlayStoreSubscription subscription, string productIdentifier)
    {
        var (subscriptionId, basePlanId) = ParseProductIdentifier(productIdentifier);

        return subscriptionId == subscription.SubscriptionId
            && (subscription.BasePlanId is null || basePlanId == subscription.BasePlanId);
    }

    // Google Play tells an upgrade from a downgrade by price per unit of time, and allows CHARGE_PRORATED_PRICE
    // only for an upgrade. In sandbox, plans renew on an accelerated schedule (a yearly plan every 30 minutes, a
    // monthly one every 5) and RevenueCat found Google compares the price per renewal minute there.
    internal static long? GetComparablePrice(long priceAmountMicros, double? periodInMonths, bool isSandbox)
    {
        if (periodInMonths is not { } months)
        {
            return null;
        }

        if (isSandbox)
        {
            return priceAmountMicros / GetSandboxRenewalMinutes(months);
        }

        return (long)(priceAmountMicros / months);
    }

    internal static bool IsUpgrade(long? oldComparablePrice, long? newComparablePrice)
    {
        return oldComparablePrice is not null
            && newComparablePrice is not null
            && newComparablePrice > oldComparablePrice;
    }

    private static long GetSandboxRenewalMinutes(double periodInMonths)
    {
        return periodInMonths switch
        {
            >= 12 => 30,
            >= 6 => 15,
            >= 3 => 10,
            _ => 5,
        };
    }
}
