using Foundation;
using Maui.RevenueCat.iOS;
using UIKit;

namespace BindingLinkerCheck;

/// <summary>
/// Touches every public binding type the iOS registrar would consider so the
/// .NET linker keeps them reachable. The interesting failure mode is
/// link-time, not run-time — this app is built (with codesigning disabled)
/// against `ios-arm64` in CI and never actually launched.
/// </summary>
public static class Program
{
    static void Main(string[] args)
    {
        TouchBindingSurface();
        // Standard iOS entry. We never reach here in CI — the build pipeline
        // only links the binary.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }

    private static void TouchBindingSurface()
    {
        // Reference every binding type. typeof() forces the .NET linker to
        // keep the class metadata, which keeps the [Register] attribute
        // alive, which makes the iOS registrar emit `_OBJC_CLASS_$_X`
        // references in registrar.o. The native linker then has to resolve
        // each one against RevenueCat.framework's symbol table.
        _ = typeof(RCAdDisplayed);
        _ = typeof(RCAdFailedToLoad);
        _ = typeof(RCAdFormat);
        _ = typeof(RCAdLoaded);
        _ = typeof(RCAdOpened);
        _ = typeof(RCAdRevenue);
        _ = typeof(RCAdRevenuePrecision);
        _ = typeof(RCAdTracker);
        _ = typeof(RCAttribution);
        _ = typeof(RCConfiguration);
        _ = typeof(RCConfigurationBuilder);
        _ = typeof(RCCustomPaywallImpressionParams);
        _ = typeof(RCCustomerInfo);
        _ = typeof(RCDangerousSettings);
        _ = typeof(RCEntitlementInfo);
        _ = typeof(RCEntitlementInfos);
        _ = typeof(RCIntroEligibility);
        _ = typeof(RCMediatorName);
        _ = typeof(RCNonSubscriptionTransaction);
        _ = typeof(RCOffering);
        _ = typeof(RCOfferings);
        _ = typeof(RCPackage);
        _ = typeof(RCPlatformInfo);
        _ = typeof(RCPresentedOfferingContext);
        _ = typeof(RCProductPaidPrice);
        _ = typeof(RCPromotionalOffer);
        _ = typeof(RCPromotionalOfferSignedData);
        _ = typeof(RCPurchaseParams);
        _ = typeof(RCPurchaseParamsBuilder);
        _ = typeof(RCPurchases);
        _ = typeof(RCPurchasesDelegate);
        _ = typeof(RCPurchasesDiagnostics);
        _ = typeof(RCStoreProduct);
        _ = typeof(RCStoreProductDiscount);
        _ = typeof(RCStoreTransaction);
        _ = typeof(RCStorefront);
        _ = typeof(RCSubscriptionInfo);
        _ = typeof(RCSubscriptionPeriod);
        _ = typeof(RCTargetingContext);
        _ = typeof(RCVirtualCurrencies);
        _ = typeof(RCVirtualCurrency);
        _ = typeof(RCWebPurchaseRedemption);
        _ = typeof(RCWinBackOffer);
    }
}

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
}
