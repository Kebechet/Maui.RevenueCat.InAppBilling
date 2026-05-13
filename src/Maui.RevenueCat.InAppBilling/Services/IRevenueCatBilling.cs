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
    Task<Dictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken = default);
    Task<List<OfferingDto>> GetOfferings(bool forceRefresh = false, CancellationToken cancellationToken = default);
    Task<PurchaseResultDto> PurchaseProduct(PackageDto packageToPurchase, CancellationToken cancellationToken = default);
    Task<List<string>> GetActiveSubscriptions(CancellationToken cancellationToken = default);
    Task<List<string>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken = default);
    Task<DateTime?> GetPurchaseDateForProductIdentifier(string productSku, CancellationToken cancellationToken = default);
    Task<string?> GetManagementSubscriptionUrl(CancellationToken cancellationToken = default);
    Task<CustomerInfoDto?> Login(string appUserId, CancellationToken cancellationToken = default);
    Task<CustomerInfoDto?> Logout(CancellationToken cancellationToken = default);
    Task<CustomerInfoDto?> RestoreTransactions(CancellationToken cancellationToken = default);
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
