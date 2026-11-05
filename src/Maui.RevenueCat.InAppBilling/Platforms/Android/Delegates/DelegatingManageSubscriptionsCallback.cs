using Com.Revenuecat.Purchases;
using Com.Revenuecat.Purchases.Interfaces;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;

internal sealed class DelegatingManageSubscriptionsCallback : DelegatingListenerBase<bool>,
        IManageSubscriptionsCallback
{
    public DelegatingManageSubscriptionsCallback(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnError(PurchasesError error)
    {
        ReportException(new PurchasesErrorException(error, false));
    }

    public void OnSuccess()
    {
        ReportSuccess(true);
    }
}
