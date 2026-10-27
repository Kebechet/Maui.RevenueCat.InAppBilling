namespace Maui.RevenueCat.InAppBilling.Platforms.Android;

internal sealed record ActivePlayStoreSubscription(string SubscriptionId, string? BasePlanId, bool IsSandbox);

internal enum SubscriptionReplacementMode
{
    ChargeProratedPrice,
    Deferred,
    ChargeFullPrice,
}

// Google Play has no subscription groups, so a second subscription product bought without naming the one it
// replaces runs in parallel and the user pays for both. These are the rules RevenueCat's own Paywalls use to
// pick the replacement when a paywall has product changes enabled:
// https://github.com/RevenueCat/purchases-android/blob/10.16.1/ui/revenuecatui/src/main/kotlin/com/revenuecat/purchases/ui/revenuecatui/data/ProductChangeCalculator.kt
// The reasoning behind them is in https://github.com/RevenueCat/purchases-android/pull/3062
// Kept free of Android types so DemoHarness.Tests can compile and test it on plain .NET.
internal static class SubscriptionReplacementRules
{
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

    internal static bool IsProductOf(ActivePlayStoreSubscription subscription, string subscriptionId, string? basePlanId)
    {
        return subscriptionId == subscription.SubscriptionId
            && (subscription.BasePlanId is null || basePlanId == subscription.BasePlanId);
    }

    // Google Play tells an upgrade from a downgrade by price per unit of time, and allows CHARGE_PRORATED_PRICE
    // only for an upgrade. In sandbox, plans renew on an accelerated schedule (a yearly plan every 30 minutes, a
    // monthly one every 5) and RevenueCat found Google compares the price per renewal minute there.
    internal static long? GetComparablePrice(long priceAmountMicros, double? periodInMonths, bool isSandbox)
    {
        if (periodInMonths is null)
        {
            return null;
        }

        if (isSandbox)
        {
            return priceAmountMicros / GetSandboxRenewalMinutes(periodInMonths.Value);
        }

        return (long)(priceAmountMicros / periodInMonths.Value);
    }

    internal static bool IsUpgrade(long? oldComparablePrice, long? newComparablePrice)
    {
        return oldComparablePrice is not null
            && newComparablePrice is not null
            && newComparablePrice.Value > oldComparablePrice.Value;
    }

    internal static SubscriptionReplacementMode ChooseReplacementMode(
        bool isNewPlanPrepaid, long? oldComparablePrice, long? newComparablePrice)
    {
        // Google Play fails a switch to a prepaid plan with any mode other than CHARGE_FULL_PRICE. RevenueCat's
        // Paywalls don't handle this: https://developer.android.com/google/play/billing/subscriptions
        if (isNewPlanPrepaid)
        {
            return SubscriptionReplacementMode.ChargeFullPrice;
        }

        return IsUpgrade(oldComparablePrice, newComparablePrice)
            ? SubscriptionReplacementMode.ChargeProratedPrice
            : SubscriptionReplacementMode.Deferred;
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
