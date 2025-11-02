using Com.Revenuecat.Purchases.Models;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class StoreTransactionExtensions
{
    internal static StoreTransactionDto ToStoreTransactionDto(this StoreTransaction storeTransaction)
    {
        return new StoreTransactionDto
        {
            ProductIdentifier = storeTransaction.ProductIds.FirstOrDefault() ?? string.Empty,
            PurchaseDate = storeTransaction.PurchaseTime.ToDateTime(),
            TransactionIdentifier = storeTransaction.PurchaseToken,
            Quantity = storeTransaction.ProductIds.Count
        };
    }
}
