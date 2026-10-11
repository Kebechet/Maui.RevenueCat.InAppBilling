using Com.Revenuecat.Purchases.Interfaces;
using Com.Revenuecat.Purchases.Models;
using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;

internal sealed class DelegatingGetStoreProductsCallback : DelegatingListenerBase<IList<IStoreProduct>>,
        IGetStoreProductsCallback
{
    public DelegatingGetStoreProductsCallback(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnError(PurchasesError error)
    {
        ReportException(new PurchasesErrorException(error, false));
    }

    public void OnReceived(IList<IStoreProduct> storeProducts)
    {
        ReportSuccess(storeProducts);
    }
}
