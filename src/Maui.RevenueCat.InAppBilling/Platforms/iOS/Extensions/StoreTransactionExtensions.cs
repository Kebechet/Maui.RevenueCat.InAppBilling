using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class StoreTransactionExtensions
{
    internal static StoreTransactionDto ToStoreTransactionDto(this RCStoreTransaction storeTransaction)
    {
        return new StoreTransactionDto
        {
            ProductIdentifier = storeTransaction.ProductIdentifier,
            PurchaseDate = storeTransaction.PurchaseDate.ToDateTime() ?? DateTime.UtcNow,
            TransactionIdentifier = storeTransaction.TransactionIdentifier,
            Quantity = storeTransaction.Quantity
        };
    }
}
