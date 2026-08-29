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
    Task<string?> GetManagementSubscriptionUrl(CancellationToken cancellationToken = default);
    Task<CustomerInfoDto?> Login(string appUserId, CancellationToken cancellationToken = default);
    Task<CustomerInfoDto?> Logout(CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores previous purchases for the current App User ID.
    /// </summary>
    /// <remarks>
    /// On success <see cref="PurchaseResultDto.CustomerInfo"/> holds the refreshed customer info -
    /// still null-check it, <see cref="PurchaseResultDto.IsSuccess"/> does not narrow the type.
    /// On failure <see cref="PurchaseResultDto.ErrorStatus"/> is the stable value to branch on and
    /// <see cref="PurchaseResultDto.ErrorMessage"/> carries the underlying store SDK detail for logs,
    /// except on cancellation where it stays null. Restore has no user-facing cancel, so
    /// <see cref="PurchaseErrorStatus.PurchaseCancelledError"/> here means the supplied
    /// <paramref name="cancellationToken"/> fired - not that the user cancelled anything.
    /// <see cref="PurchaseResultDto.Transaction"/> is never set - restore reports no single transaction.
    /// </remarks>
    Task<PurchaseResultDto> RestoreTransactions(CancellationToken cancellationToken = default);
    Task<CustomerInfoDto?> GetCustomerInfo(CancellationToken cancellationToken = default);

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
