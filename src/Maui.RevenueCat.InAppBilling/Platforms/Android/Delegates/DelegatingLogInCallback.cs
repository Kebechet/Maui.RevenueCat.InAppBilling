using Com.Revenuecat.Purchases.Interfaces;
using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;

internal sealed class DelegatingLogInCallback : DelegatingListenerBase<CustomerInfo>, ILogInCallback
{
    public DelegatingLogInCallback(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnError(PurchasesError error)
    {
        ReportException(new PurchasesErrorException(error, false));
    }

    public void OnReceived(CustomerInfo customerInfo, bool created)
    {
        ReportSuccess(customerInfo);
    }
}