using Android.App;
using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class PurchasesExtensions
{
    public static Task<CustomerInfo> GetCustomerInfoAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveCustomerInfoCallback(cancellationToken);
        purchases.GetCustomerInfo(listener);
        return listener.Task;
    }

    public static Task<CustomerInfo> LogInAsync(this Purchases purchases, string newAppUserId,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingLogInCallback(cancellationToken);
        purchases.LogIn(newAppUserId, listener);
        return listener.Task;
    }

    public static Task<CustomerInfo> LogOutAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveCustomerInfoCallback(cancellationToken);
        purchases.LogOut(listener);
        return listener.Task;
    }

    public static Task<Offerings> GetOfferingsAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveOfferingsCallback(cancellationToken);
        purchases.GetOfferings(listener);
        return listener.Task;
    }

    public static Task<PurchaseSuccessInfo> PurchaseAsync(this Purchases purchases, Activity activity,
        Package packageToPurchase, CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingMakePurchaseListener(cancellationToken);
        var purchaseParams = new PurchaseParams(new PurchaseParams.Builder(activity, packageToPurchase));
        purchases.Purchase(purchaseParams, listener);
        return listener.Task;
    }

    public static Task<CustomerInfo> RestorePurchasesAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveCustomerInfoCallback(cancellationToken);
        purchases.RestorePurchases(listener);
        return listener.Task;
    }
}
