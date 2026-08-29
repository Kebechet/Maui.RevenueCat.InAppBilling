using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Services;

public interface IRevenueCatBilling
{
    bool IsInitialized();
    bool IsAnonymous();
    string GetAppUserId();
    Task<bool> CanMakePayments(CancellationToken cancellationToken = default);

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
    Task<Dictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken = default);
    Task<List<OfferingDto>> GetOfferings(bool forceRefresh = false, CancellationToken cancellationToken = default);
    Task<PurchaseResultDto> PurchaseProduct(PackageDto packageToPurchase, CancellationToken cancellationToken = default);
    Task<List<string>> GetActiveSubscriptions(CancellationToken cancellationToken = default);
    Task<List<string>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken = default);
    Task<DateTime?> GetPurchaseDateForProductIdentifier(string productSku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the store's subscription-management URL for the current user.
    /// </summary>
    /// <remarks>
    /// A success with a null <see cref="ManagementUrlResultDto.ManagementUrl"/> means the user has
    /// no store-managed subscription; that is distinct from a failure, which reports
    /// <see cref="ManagementUrlResultDto.ErrorStatus"/>.
    /// </remarks>
    Task<ManagementUrlResultDto> GetManagementSubscriptionUrl(CancellationToken cancellationToken = default);
    /// <summary>
    /// Logs in an identified user, aliasing any anonymous ID onto it.
    /// </summary>
    /// <inheritdoc cref="RestoreTransactions" path="/remarks"/>
    Task<CustomerInfoResultDto> Login(string appUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs the current user out and returns to a fresh anonymous App User ID.
    /// </summary>
    /// <inheritdoc cref="RestoreTransactions" path="/remarks"/>
    Task<CustomerInfoResultDto> Logout(CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores previous purchases for the current App User ID.
    /// </summary>
    /// <remarks>
    /// On success <see cref="CustomerInfoResultDto.CustomerInfo"/> holds the customer info - still
    /// null-check it, <see cref="CustomerInfoResultDto.IsSuccess"/> does not narrow the type.
    /// On failure <see cref="CustomerInfoResultDto.ErrorStatus"/> is the stable value to branch on
    /// and <see cref="CustomerInfoResultDto.ErrorMessage"/> carries the underlying store SDK detail
    /// for logs, except on cancellation where it stays null. None of these operations has a
    /// user-facing cancel, so <see cref="PurchaseErrorStatus.PurchaseCancelledError"/> means the
    /// supplied <paramref name="cancellationToken"/> fired - not that the user cancelled anything.
    /// </remarks>
    Task<CustomerInfoResultDto> RestoreTransactions(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the current customer info and entitlements.
    /// </summary>
    /// <inheritdoc cref="RestoreTransactions" path="/remarks"/>
    Task<CustomerInfoResultDto> GetCustomerInfo(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the ISO 3166-1 alpha-2 country code of the user's App Store / Play Store
    /// storefront (e.g. <c>"US"</c>, <c>"CZ"</c>), or an empty string if it can't be
    /// resolved. Independent of the device's <see cref="System.Globalization.CultureInfo.CurrentCulture"/>.
    /// </summary>
    Task<string> GetStorefrontCountryCode(CancellationToken cancellationToken = default);

    // Subscriber Attributes
    void SetEmail(string email);
    void SetDisplayName(string name);
    void SetPhoneNumber(string phone);
    void SetAttributes(IDictionary<string, string> attributes);
}
