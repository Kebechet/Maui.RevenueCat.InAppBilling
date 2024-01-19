using Foundation;
using Maui.RevenueCat.iOS;
using Maui.RevenueCat.iOS.Additions;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Exceptions;

public class PurchasesErrorException : Exception
{
    public NSError? PurchasesError { get; }
    public bool UserCancelled { get; }

    public NSObject? ReadableErrorCode { get; }
    public NSObject? UnderlyingError { get; }
    public string? LocalizedDescription { get; }
    public RCPurchasesErrorCode PurchasesErrorCode { get; }

    public PurchasesErrorException(NSError? purchasesError, bool userCancelled)
        : base($"{purchasesError?.Description} userCancelled: {userCancelled}", WrapError(purchasesError))
    {
        PurchasesError = purchasesError;
        UserCancelled = userCancelled;
        if (purchasesError is not null)
        {
            purchasesError.UserInfo.TryGetValue(ErrorDetails.ReadableErrorCodeKey, out var readableErrorCode);
            ReadableErrorCode = readableErrorCode;
            purchasesError.UserInfo.TryGetValue(NSError.UnderlyingErrorKey, out var underlyingError);
            UnderlyingError = underlyingError;
            var localizedDescription = purchasesError.LocalizedDescription;
            LocalizedDescription = localizedDescription;

            var purchaseErrorCodeInt = (int)purchasesError.Code;
            PurchasesErrorCode = (RCPurchasesErrorCode)purchaseErrorCodeInt;
        }
    }

    private static NSErrorException? WrapError(NSError? purchasesError)
    {
        if (purchasesError is not null)
        {
            return new NSErrorException(purchasesError);
        }

        return null;
    }
}
