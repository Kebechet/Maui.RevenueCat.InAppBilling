using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Services;

/// <summary>
/// Cross-platform RevenueCat billing API.
/// </summary>
/// <remarks>
/// Every asynchronous member returns a <see cref="DataResult{TValue, TError}"/> keyed on
/// <see cref="PurchaseErrorStatus"/>. Read <c>Value</c> only when <c>IsSuccess</c> is true - it is
/// declared nullable and <c>IsSuccess</c> does not narrow it. On failure, <c>Error</c> is the
/// closed, documented failure mode to branch on and <c>ErrorException</c> carries the raw store SDK
/// exception for logs and diagnostics; do not surface it to users.
/// <para>
/// Exceptions are still thrown for developer mistakes, such as calling a member before
/// <see cref="Initialize(string)"/>.
/// </para>
/// <para>
/// None of these operations has a user-facing cancel affordance, so
/// <see cref="PurchaseErrorStatus.PurchaseCancelledError"/> means the supplied
/// <c>CancellationToken</c> fired - except on <see cref="PurchaseProduct"/>, where it is the user
/// dismissing the store sheet.
/// </para>
/// </remarks>
public interface IRevenueCatBilling
{
    bool IsInitialized();
    bool IsAnonymous();
    string GetAppUserId();
    Task<DataResult<bool, PurchaseErrorStatus>> CanMakePayments(CancellationToken cancellationToken = default);

    void Initialize(string apiKey);

    /// <summary>
    /// Initializes RevenueCat with a custom App User ID so no anonymous user
    /// (<c>$RCAnonymousID:...</c>) is created. Prefer this overload when the user's ID is
    /// already known at startup; otherwise use <see cref="Initialize(string)"/> and call
    /// <see cref="Login(string, CancellationToken)"/> once the user is identified (which aliases the anonymous ID).
    /// </summary>
    void Initialize(string apiKey, string appUserId);

    /// <summary>
    /// Checks trial or introductory discount eligibility for the given product identifiers.
    /// Apple platforms only (iOS and Mac Catalyst): the underlying RevenueCat API has no
    /// Android equivalent, so the Android implementation throws <see cref="NotImplementedException"/>.
    /// </summary>
    Task<DataResult<Dictionary<string, IntroElegibilityStatus>, PurchaseErrorStatus>> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken = default);

    Task<DataResult<List<OfferingDto>, PurchaseErrorStatus>> GetOfferings(bool forceRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the store purchase flow for a package previously returned by <see cref="GetOfferings"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="PurchaseErrorStatus.PurchaseCancelledError"/> here means the user dismissed the
    /// store sheet, which is an expected outcome rather than a fault.
    /// <see cref="PurchaseErrorStatus.PaymentPendingError"/> means the store accepted the purchase
    /// but it has not settled yet.
    /// </remarks>
    Task<PurchaseResultDto> PurchaseProduct(PackageDto packageToPurchase, CancellationToken cancellationToken = default);

    Task<DataResult<List<string>, PurchaseErrorStatus>> GetActiveSubscriptions(CancellationToken cancellationToken = default);
    Task<DataResult<List<string>, PurchaseErrorStatus>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken = default);

    /// <summary>
    /// Purchase date for a product the user owns. A success with a null <c>Value</c> means the
    /// product was never purchased, which is distinct from a failure.
    /// </summary>
    Task<DataResult<DateTime?, PurchaseErrorStatus>> GetPurchaseDateForProductIdentifier(string productSku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the store's subscription-management URL for the current user. A success with a
    /// null <c>Value</c> means the user has no store-managed subscription, which is distinct from
    /// a failure.
    /// </summary>
    Task<DataResult<string, PurchaseErrorStatus>> GetManagementSubscriptionUrl(CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs in an identified user, aliasing any anonymous ID onto it.
    /// </summary>
    Task<DataResult<CustomerInfoDto, PurchaseErrorStatus>> Login(string appUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs the current user out and returns to a fresh anonymous App User ID.
    /// </summary>
    Task<DataResult<CustomerInfoDto, PurchaseErrorStatus>> Logout(CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores previous purchases for the current App User ID.
    /// </summary>
    Task<DataResult<CustomerInfoDto, PurchaseErrorStatus>> RestoreTransactions(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the current customer info and entitlements.
    /// </summary>
    Task<DataResult<CustomerInfoDto, PurchaseErrorStatus>> GetCustomerInfo(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the ISO 3166-1 alpha-2 country code of the user's App Store / Play Store
    /// storefront (e.g. <c>"US"</c>, <c>"CZ"</c>). Independent of the device's
    /// <see cref="System.Globalization.CultureInfo.CurrentCulture"/>.
    /// </summary>
    Task<DataResult<string, PurchaseErrorStatus>> GetStorefrontCountryCode(CancellationToken cancellationToken = default);

    // Subscriber Attributes
    void SetEmail(string email);
    void SetDisplayName(string name);
    void SetPhoneNumber(string phone);
    void SetAttributes(IDictionary<string, string> attributes);
}
