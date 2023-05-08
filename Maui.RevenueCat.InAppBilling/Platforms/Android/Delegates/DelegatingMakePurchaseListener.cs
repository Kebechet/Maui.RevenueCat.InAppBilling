using Com.Revenuecat.Purchases.Interfaces;
using Com.Revenuecat.Purchases.Models;
using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Models;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;

internal class DelegatingMakePurchaseListener : DelegatingListenerBase<PurchaseSuccessInfo>, IPurchaseCallback
{
    public DelegatingMakePurchaseListener(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnCompleted(StoreTransaction storeTransaction, CustomerInfo customerInfo)
    {
        ReportSuccess(new PurchaseSuccessInfo(storeTransaction, customerInfo));
    }

    public void OnError(PurchasesError purchasesError, bool userCancelled)
    {
        ReportException(new PurchasesErrorException(purchasesError, userCancelled));
    }
}