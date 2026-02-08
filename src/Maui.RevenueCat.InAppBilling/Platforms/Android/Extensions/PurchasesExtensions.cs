using Android.App;
using Android.Content;
using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class PurchasesExtensions
{
    internal static async Task<bool> CanMakePaymentsAsync(this Purchases purchases, Context context,
        CancellationToken cancellationToken = default)
    {
        var callback = new DelegatingCallback<Java.Lang.Boolean>(cancellationToken);
        Purchases.CanMakePayments(context, callback);
        var result = await callback.Task;
        return result.BooleanValue();
    }

    internal static Task<CustomerInfo> GetCustomerInfoAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveCustomerInfoCallback(cancellationToken);
        purchases.GetCustomerInfo(listener);
        return listener.Task;
    }

    internal static Task<CustomerInfo> LogInAsync(this Purchases purchases, string newAppUserId,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingLogInCallback(cancellationToken);
        purchases.LogIn(newAppUserId, listener);
        return listener.Task;
    }

    internal static Task<CustomerInfo> LogOutAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveCustomerInfoCallback(cancellationToken);
        purchases.LogOut(listener);
        return listener.Task;
    }

    internal static Task<Offerings> GetOfferingsAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveOfferingsCallback(cancellationToken);
        purchases.GetOfferings(listener);
        return listener.Task;
    }

    internal static Task<PurchaseSuccessInfo> PurchaseAsync(this Purchases purchases, Activity activity,
        Package packageToPurchase, CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingMakePurchaseListener(cancellationToken);
        var purchaseParams = new PurchaseParams(new PurchaseParams.Builder(activity, packageToPurchase));
        purchases.Purchase(purchaseParams, listener);
        return listener.Task;
    }

    internal static Task<CustomerInfo> RestorePurchasesAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveCustomerInfoCallback(cancellationToken);
        purchases.RestorePurchases(listener);
        return listener.Task;
    }
}
