using Com.Revenuecat.Purchases;
using Com.Revenuecat.Purchases.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Models;

public class PurchaseSuccessInfo
{
    public StoreTransaction StoreTransaction { get; }
    public CustomerInfo CustomerInfo { get; }

    public PurchaseSuccessInfo(StoreTransaction storeTransaction, CustomerInfo customerInfo)
    {
        StoreTransaction = storeTransaction;
        CustomerInfo = customerInfo;
    }
}
