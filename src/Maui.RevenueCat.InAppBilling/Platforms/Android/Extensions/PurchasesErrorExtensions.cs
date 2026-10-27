using Com.Revenuecat.Purchases;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class PurchasesErrorExtensions
{
    internal static bool IsNoExistingPurchaseError(this PurchasesError purchasesError)
    {
        return purchasesError.Code == PurchasesErrorCode.PurchaseInvalidError
            && SubscriptionReplacementRules.IsNoExistingPurchaseMessage(purchasesError.UnderlyingErrorMessage);
    }
}
