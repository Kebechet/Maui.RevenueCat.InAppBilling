using System.Runtime.Serialization;

namespace Maui.RevenueCat.InAppBilling.Enums;

//https://github.com/RevenueCat/purchases-android/blob/main/public/src/main/java/com/revenuecat/purchases/errors.kt
//https://github.com/RevenueCat/purchases-ios/blob/main/Sources/Error%20Handling/ErrorCode.swift#L39
//https://www.revenuecat.com/docs/test-and-launch/errors
public enum PurchaseErrorStatus
{
    [EnumMember(Value = "UNKNOWN")]
    UnknownError = 0,
    [EnumMember(Value = "PURCHASE_CANCELLED")]
    PurchaseCancelledError = 1,
    [EnumMember(Value = "STORE_PROBLEM")]
    StoreProblemError = 2,
    [EnumMember(Value = "PURCHASE_NOT_ALLOWED")]
    PurchaseNotAllowedError = 3,
    [EnumMember(Value = "PURCHASE_INVALID")]
    PurchaseInvalidError = 4,
    [EnumMember(Value = "PRODUCT_NOT_AVAILABLE_FOR_PURCHASE")]
    ProductNotAvailableForPurchaseError = 5,
    [EnumMember(Value = "PRODUCT_ALREADY_PURCHASED")]
    ProductAlreadyPurchasedError = 6,
    [EnumMember(Value = "RECEIPT_ALREADY_IN_USE")]
    ReceiptAlreadyInUseError = 7,
    [EnumMember(Value = "INVALID_RECEIPT")]
    InvalidReceiptError = 8,
    [EnumMember(Value = "MISSING_RECEIPT_FILE")]
    MissingReceiptFileError = 9,
    [EnumMember(Value = "NETWORK_ERROR")]
    NetworkError = 10,
    [EnumMember(Value = "INVALID_CREDENTIALS")]
    InvalidCredentialsError = 11,
    [EnumMember(Value = "UNEXPECTED_BACKEND_RESPONSE_ERROR")]
    UnexpectedBackendResponseError = 12,
    [EnumMember(Value = "RECEIPT_IN_USE_BY_OTHER_SUBSCRIBER")]
    ReceiptInUseByOtherSubscriberError = 13,
    [EnumMember(Value = "INVALID_APP_USER_ID")]
    InvalidAppUserIdError = 14,
    [EnumMember(Value = "OPERATION_ALREADY_IN_PROGRESS_FOR_PRODUCT_ERROR")]
    OperationAlreadyInProgressForProductError = 15,
    [EnumMember(Value = "UNKNOWN_BACKEND_ERROR")]
    UnknownBackendError = 16,
    [EnumMember(Value = "INVALID_APPLE_SUBSCRIPTION_KEY")]
    InvalidAppleSubscriptionKeyError = 17,
    [EnumMember(Value = "INELIGIBLE_ERROR")]
    IneligibleError = 18,
    [EnumMember(Value = "INSUFFICIENT_PERMISSIONS_ERROR")]
    InsufficientPermissionsError = 19,
    [EnumMember(Value = "PAYMENT_PENDING_ERROR")]
    PaymentPendingError = 20,
    [EnumMember(Value = "INVALID_SUBSCRIBER_ATTRIBUTES")]
    InvalidSubscriberAttributesError = 21,
    [EnumMember(Value = "LOGOUT_CALLED_WITH_ANONYMOUS_USER")]
    LogOutAnonymousUserError = 22,
    [EnumMember(Value = "CONFIGURATION_ERROR")]
    ConfigurationError = 23,
    [EnumMember(Value = "UNSUPPORTED_ERROR")]
    UnsupportedError = 24,
    [EnumMember(Value = "EMPTY_SUBSCRIBER_ATTRIBUTES")]
    EmptySubscriberAttributesError = 25,
    [EnumMember(Value = "PRODUCT_DISCOUNT_MISSING_IDENTIFIER_ERROR")]
    ProductDiscountMissingIdentifierError = 26,
    //27 is not specified anywhere
    [EnumMember(Value = "PRODUCT_DISCOUNT_MISSING_SUBSCRIPTION_GROUP_IDENTIFIER_ERROR")]
    ProductDiscountMissingSubscriptionGroupIdentifierError = 28,
    [EnumMember(Value = "CUSTOMER_INFO_ERROR")]
    CustomerInfoError = 29,
    [EnumMember(Value = "SYSTEM_INFO_ERROR")]
    SystemInfoError = 30,
    [EnumMember(Value = "BEGIN_REFUND_REQUEST_ERROR")]
    BeginRefundRequestError = 31,
    [EnumMember(Value = "PRODUCT_REQUEST_TIMED_OUT_ERROR")]
    ProductRequestTimedOut = 32,
    [EnumMember(Value = "API_ENDPOINT_BLOCKED_ERROR")]
    APIEndpointBlocked = 33,
    [EnumMember(Value = "INVALID_PROMOTIONAL_OFFER_ERROR")]
    InvalidPromotionalOfferError = 34,
    [EnumMember(Value = "OFFLINE_CONNECTION_ERROR")]
    OfflineConnectionError = 35,
    [EnumMember(Value = "FEATURE_NOT_AVAILABLE_IN_CUSTOM_ENTITLEMENTS_COMPUTATION_MODE_ERROR")]
    FeatureNotAvailableInCustomEntitlementsComputationMode = 36,
    [EnumMember(Value = "SIGNATURE_VERIFICATION_FAILED")]
    SignatureVerificationFailed = 37,
}