using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Models;

public sealed class PurchaseSuccessInfo
{
    public RCStoreTransaction StoreTransaction { get; }
    public RCCustomerInfo CustomerInfo { get; }

    public PurchaseSuccessInfo(RCStoreTransaction transaction, RCCustomerInfo customerInfo)
    {
        StoreTransaction = transaction;
        CustomerInfo = customerInfo;
    }
}
