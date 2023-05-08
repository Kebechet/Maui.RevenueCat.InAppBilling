using Com.Revenuecat.Purchases.Interfaces;
using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;

internal class DelegatingReceiveOfferingsCallback : DelegatingListenerBase<Offerings>, IReceiveOfferingsCallback
{
    public DelegatingReceiveOfferingsCallback(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnError(PurchasesError purchasesError)
    {
        ReportException(new PurchasesErrorException(purchasesError, false));
    }

    public void OnReceived(Offerings offerings)
    {
        ReportSuccess(offerings);
    }
}
