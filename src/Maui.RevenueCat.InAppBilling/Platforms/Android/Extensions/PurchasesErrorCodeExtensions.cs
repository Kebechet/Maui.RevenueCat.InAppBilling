using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class PurchasesErrorCodeExtensions
{
    /// <summary>
    /// Maps an Android <see cref="PurchasesErrorCode"/> to the wrapper's
    /// <see cref="PurchaseErrorStatus"/> by name. Avoids a raw
    /// <c>(PurchaseErrorStatus)(int)code.Code</c> cast — the Android Kotlin
    /// enum uses different integer values than its iOS counterpart for the
    /// same conceptual error (issue #106: e.g. Android's <c>CustomerInfoError</c>
    /// has code 28, iOS's has code 29). Switching on the enum value itself
    /// avoids that misalignment.
    /// </summary>
    internal static PurchaseErrorStatus ToPurchaseErrorStatus(this PurchasesErrorCode code) => code switch
    {
        var c when c == PurchasesErrorCode.UnknownError                       => PurchaseErrorStatus.UnknownError,
        var c when c == PurchasesErrorCode.PurchaseCancelledError             => PurchaseErrorStatus.PurchaseCancelledError,
        var c when c == PurchasesErrorCode.StoreProblemError                  => PurchaseErrorStatus.StoreProblemError,
        var c when c == PurchasesErrorCode.PurchaseNotAllowedError            => PurchaseErrorStatus.PurchaseNotAllowedError,
        var c when c == PurchasesErrorCode.PurchaseInvalidError               => PurchaseErrorStatus.PurchaseInvalidError,
        var c when c == PurchasesErrorCode.ProductNotAvailableForPurchaseError => PurchaseErrorStatus.ProductNotAvailableForPurchaseError,
        var c when c == PurchasesErrorCode.ProductAlreadyPurchasedError       => PurchaseErrorStatus.ProductAlreadyPurchasedError,
        var c when c == PurchasesErrorCode.ReceiptAlreadyInUseError           => PurchaseErrorStatus.ReceiptAlreadyInUseError,
        var c when c == PurchasesErrorCode.InvalidReceiptError                => PurchaseErrorStatus.InvalidReceiptError,
        var c when c == PurchasesErrorCode.MissingReceiptFileError            => PurchaseErrorStatus.MissingReceiptFileError,
        var c when c == PurchasesErrorCode.NetworkError                       => PurchaseErrorStatus.NetworkError,
        var c when c == PurchasesErrorCode.InvalidCredentialsError            => PurchaseErrorStatus.InvalidCredentialsError,
        var c when c == PurchasesErrorCode.UnexpectedBackendResponseError     => PurchaseErrorStatus.UnexpectedBackendResponseError,
        var c when c == PurchasesErrorCode.InvalidAppUserIdError              => PurchaseErrorStatus.InvalidAppUserIdError,
        var c when c == PurchasesErrorCode.OperationAlreadyInProgressError    => PurchaseErrorStatus.OperationAlreadyInProgressForProductError,
        var c when c == PurchasesErrorCode.UnknownBackendError                => PurchaseErrorStatus.UnknownBackendError,
        var c when c == PurchasesErrorCode.InvalidAppleSubscriptionKeyError   => PurchaseErrorStatus.InvalidAppleSubscriptionKeyError,
        var c when c == PurchasesErrorCode.IneligibleError                    => PurchaseErrorStatus.IneligibleError,
        var c when c == PurchasesErrorCode.InsufficientPermissionsError       => PurchaseErrorStatus.InsufficientPermissionsError,
        var c when c == PurchasesErrorCode.PaymentPendingError                => PurchaseErrorStatus.PaymentPendingError,
        var c when c == PurchasesErrorCode.InvalidSubscriberAttributesError   => PurchaseErrorStatus.InvalidSubscriberAttributesError,
        var c when c == PurchasesErrorCode.LogOutWithAnonymousUserError       => PurchaseErrorStatus.LogOutAnonymousUserError,
        var c when c == PurchasesErrorCode.ConfigurationError                 => PurchaseErrorStatus.ConfigurationError,
        var c when c == PurchasesErrorCode.UnsupportedError                   => PurchaseErrorStatus.UnsupportedError,
        var c when c == PurchasesErrorCode.EmptySubscriberAttributesError     => PurchaseErrorStatus.EmptySubscriberAttributesError,
        var c when c == PurchasesErrorCode.CustomerInfoError                  => PurchaseErrorStatus.CustomerInfoError,
        var c when c == PurchasesErrorCode.SignatureVerificationError         => PurchaseErrorStatus.SignatureVerificationFailed,
        var c when c == PurchasesErrorCode.TestStoreSimulatedPurchaseError    => PurchaseErrorStatus.TestStoreSimulatedPurchaseError,
        _                                                                     => PurchaseErrorStatus.UnknownError,
    };
}
