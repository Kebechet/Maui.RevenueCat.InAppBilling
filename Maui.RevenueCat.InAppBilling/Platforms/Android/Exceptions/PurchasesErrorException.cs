using Com.Revenuecat.Purchases;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;

public class PurchasesErrorException : Exception
{
    public PurchasesError PurchasesError { get; }
    public bool UserCancelled { get; }

    public PurchasesErrorException(PurchasesError purchasesError, bool userCancelled)
        : base($"{purchasesError?.Message} ({purchasesError?.UnderlyingErrorMessage}) code: {purchasesError?.Code} userCancelled: {userCancelled}")
    {
        PurchasesError = purchasesError;
        UserCancelled = userCancelled;
    }
}
