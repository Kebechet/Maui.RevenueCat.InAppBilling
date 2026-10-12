using Com.Revenuecat.Purchases.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Models;

internal sealed record SubscriptionReplacement(string OldProductId, StoreReplacementMode ReplacementMode);
