using Com.Revenuecat.Purchases.Interfaces;
using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;

internal sealed class DelegatingGetStorefrontCountryCodeCallback : DelegatingListenerBase<string>, IGetStorefrontCallback
{
    public DelegatingGetStorefrontCountryCodeCallback(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnError(PurchasesError purchasesError)
    {
        ReportException(new PurchasesErrorException(purchasesError, false));
    }

    public void OnReceived(string storefrontCountryCode)
    {
        ReportSuccess(storefrontCountryCode ?? string.Empty);
    }
}
