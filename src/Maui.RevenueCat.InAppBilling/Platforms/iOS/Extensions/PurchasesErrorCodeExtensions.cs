using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class PurchasesErrorCodeExtensions
{
    /// <summary>
    /// Maps an iOS <see cref="RCPurchasesErrorCode"/> to the wrapper's
    /// <see cref="PurchaseErrorStatus"/> by name. Avoids a raw
    /// <c>(PurchaseErrorStatus)(int)</c> cast — iOS and Android emit
    /// different integer codes for the same conceptual error
    /// (see issue #106), so name-based mapping is the only safe option.
    /// </summary>
    internal static PurchaseErrorStatus ToPurchaseErrorStatus(this RCPurchasesErrorCode code) => code switch
    {
        RCPurchasesErrorCode.UnknownError                                       => PurchaseErrorStatus.UnknownError,
        RCPurchasesErrorCode.PurchaseCancelledError                             => PurchaseErrorStatus.PurchaseCancelledError,
        RCPurchasesErrorCode.StoreProblemError                                  => PurchaseErrorStatus.StoreProblemError,
        RCPurchasesErrorCode.PurchaseNotAllowedError                            => PurchaseErrorStatus.PurchaseNotAllowedError,
        RCPurchasesErrorCode.PurchaseInvalidError                               => PurchaseErrorStatus.PurchaseInvalidError,
        RCPurchasesErrorCode.ProductNotAvailableForPurchaseError                => PurchaseErrorStatus.ProductNotAvailableForPurchaseError,
        RCPurchasesErrorCode.ProductAlreadyPurchasedError                       => PurchaseErrorStatus.ProductAlreadyPurchasedError,
        RCPurchasesErrorCode.ReceiptAlreadyInUseError                           => PurchaseErrorStatus.ReceiptAlreadyInUseError,
        RCPurchasesErrorCode.InvalidReceiptError                                => PurchaseErrorStatus.InvalidReceiptError,
        RCPurchasesErrorCode.MissingReceiptFileError                            => PurchaseErrorStatus.MissingReceiptFileError,
        RCPurchasesErrorCode.NetworkError                                       => PurchaseErrorStatus.NetworkError,
        RCPurchasesErrorCode.InvalidCredentialsError                            => PurchaseErrorStatus.InvalidCredentialsError,
        RCPurchasesErrorCode.UnexpectedBackendResponseError                     => PurchaseErrorStatus.UnexpectedBackendResponseError,
        RCPurchasesErrorCode.ReceiptInUseByOtherSubscriberError                 => PurchaseErrorStatus.ReceiptInUseByOtherSubscriberError,
        RCPurchasesErrorCode.InvalidAppUserIdError                              => PurchaseErrorStatus.InvalidAppUserIdError,
        RCPurchasesErrorCode.OperationAlreadyInProgressForProductError          => PurchaseErrorStatus.OperationAlreadyInProgressForProductError,
        RCPurchasesErrorCode.UnknownBackendError                                => PurchaseErrorStatus.UnknownBackendError,
        RCPurchasesErrorCode.InvalidAppleSubscriptionKeyError                   => PurchaseErrorStatus.InvalidAppleSubscriptionKeyError,
        RCPurchasesErrorCode.IneligibleError                                    => PurchaseErrorStatus.IneligibleError,
        RCPurchasesErrorCode.InsufficientPermissionsError                       => PurchaseErrorStatus.InsufficientPermissionsError,
        RCPurchasesErrorCode.PaymentPendingError                                => PurchaseErrorStatus.PaymentPendingError,
        RCPurchasesErrorCode.InvalidSubscriberAttributesError                   => PurchaseErrorStatus.InvalidSubscriberAttributesError,
        RCPurchasesErrorCode.LogOutAnonymousUserError                           => PurchaseErrorStatus.LogOutAnonymousUserError,
        RCPurchasesErrorCode.ConfigurationError                                 => PurchaseErrorStatus.ConfigurationError,
        RCPurchasesErrorCode.UnsupportedError                                   => PurchaseErrorStatus.UnsupportedError,
        RCPurchasesErrorCode.EmptySubscriberAttributesError                     => PurchaseErrorStatus.EmptySubscriberAttributesError,
        RCPurchasesErrorCode.ProductDiscountMissingIdentifierError              => PurchaseErrorStatus.ProductDiscountMissingIdentifierError,
        RCPurchasesErrorCode.ProductDiscountMissingSubscriptionGroupIdentifierError => PurchaseErrorStatus.ProductDiscountMissingSubscriptionGroupIdentifierError,
        RCPurchasesErrorCode.CustomerInfoError                                  => PurchaseErrorStatus.CustomerInfoError,
        RCPurchasesErrorCode.SystemInfoError                                    => PurchaseErrorStatus.SystemInfoError,
        RCPurchasesErrorCode.BeginRefundRequestError                            => PurchaseErrorStatus.BeginRefundRequestError,
        RCPurchasesErrorCode.ProductRequestTimedOut                             => PurchaseErrorStatus.ProductRequestTimedOut,
        RCPurchasesErrorCode.APIEndpointBlocked                                 => PurchaseErrorStatus.APIEndpointBlocked,
        RCPurchasesErrorCode.InvalidPromotionalOfferError                       => PurchaseErrorStatus.InvalidPromotionalOfferError,
        RCPurchasesErrorCode.OfflineConnectionError                             => PurchaseErrorStatus.OfflineConnectionError,
        RCPurchasesErrorCode.FeatureNotAvailableInCustomEntitlementsComputationMode => PurchaseErrorStatus.FeatureNotAvailableInCustomEntitlementsComputationMode,
        RCPurchasesErrorCode.SignatureVerificationFailed                        => PurchaseErrorStatus.SignatureVerificationFailed,
        RCPurchasesErrorCode.FeatureNotSupportedWithStoreKit1                   => PurchaseErrorStatus.FeatureNotSupportedWithStoreKit1,
        RCPurchasesErrorCode.InvalidWebPurchaseToken                            => PurchaseErrorStatus.InvalidWebPurchaseToken,
        RCPurchasesErrorCode.PurchaseBelongsToOtherUser                         => PurchaseErrorStatus.PurchaseBelongsToOtherUser,
        RCPurchasesErrorCode.ExpiredWebPurchaseToken                            => PurchaseErrorStatus.ExpiredWebPurchaseToken,
        RCPurchasesErrorCode.TestStoreSimulatedPurchaseError                    => PurchaseErrorStatus.TestStoreSimulatedPurchaseError,
        _                                                                       => PurchaseErrorStatus.UnknownError,
    };
}
