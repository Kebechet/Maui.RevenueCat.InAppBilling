using Com.Revenuecat.Purchases.Interfaces;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Models;

internal class PurchaseResult
{
    //internal PurchaseErrorResult? ErrorResult = null;
    //internal PurchaseSuccessResult? SuccessResult = null;

    internal PurchaseErrorEventArgs? ErrorResult = null;
    internal PurchaseSuccessInfo? SuccessResult = null;
}
