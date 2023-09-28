using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Services;

public interface IRevenueCatBilling
{
    bool IsInitialized();
    bool IsAnonymous();
    string GetAppUserId();

    void Initialize(string apiKey);
    Task<Dictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibility(IList<string> identifiers, CancellationToken cancellationToken = default);
    Task<List<OfferingDto>> LoadOfferings(bool forceRefresh = false, CancellationToken cancellationToken = default);
    Task<PurchaseResult> PurchaseProduct(string offeringIdentifier, string packageIdentifier, CancellationToken cancellationToken = default);
    Task<List<string>> GetActiveSubscriptions(CancellationToken cancellationToken = default);
    Task<List<string>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken = default);
    Task<DateTime?> GetPurchaseDateForProductIdentifier(string productSku, CancellationToken cancellationToken = default);
    Task<string?> GetManagementSubscriptionUrl(CancellationToken cancellationToken = default);
    Task<CustomerInfoDto?> Login(string appUserId, CancellationToken cancellationToken = default);
    Task<CustomerInfoDto?> Logout(CancellationToken cancellationToken = default);
    Task<CustomerInfoDto?> RestoreTransactions(CancellationToken cancellationToken = default);
}
