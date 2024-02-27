using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public partial bool IsAnonymous() => true;
    public partial string GetAppUserId() => string.Empty;

    public partial void Initialize(string apiKey)
    {
        _isInitialized = true;
    }

    public async partial Task<Dictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken)
    {
        return new();
    }

    public async partial Task<List<OfferingDto>> GetOfferings(bool forceRefresh, CancellationToken cancellationToken)
    {
        return new();
    }
    public async partial Task<PurchaseResultDto> PurchaseProduct(PackageDto packageToPurchase, CancellationToken cancellationToken)
    {
        return new();
    }
    public async partial Task<List<string>> GetActiveSubscriptions(CancellationToken cancellationToken)
    {
        return new();
    }
    public async partial Task<List<string>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken)
    {
        return new();
    }
    public async partial Task<DateTime?> GetPurchaseDateForProductIdentifier(string productIdentifier, CancellationToken cancellationToken)
    {
        return DateTime.MinValue;
    }
    public async partial Task<string?> GetManagementSubscriptionUrl(CancellationToken cancellationToken)
    {
        return string.Empty;
    }
    public async partial Task<CustomerInfoDto?> Login(string appUserId, CancellationToken cancellationToken)
    {
        return new()
        {
            ActiveSubscriptions = new(),
            AllPurchasedIdentifiers = new(),
            FirstSeen = DateTime.MinValue,
            LatestExpirationDate = DateTime.MinValue,
            ManagementURL = string.Empty,
            NonConsumablePurchases = new(),
            Entitlements = new(),
        };
    }
    public async partial Task<CustomerInfoDto?> Logout(CancellationToken cancellationToken)
    {
        return new()
        {
            ActiveSubscriptions = new(),
            AllPurchasedIdentifiers = new(),
            FirstSeen = DateTime.MinValue,
            LatestExpirationDate = DateTime.MinValue,
            ManagementURL = string.Empty,
            NonConsumablePurchases = new(),
            Entitlements = new(),
        };
    }
    public async partial Task<CustomerInfoDto?> RestoreTransactions(CancellationToken cancellationToken)
    {
        return new()
        {
            ActiveSubscriptions = new(),
            AllPurchasedIdentifiers = new(),
            FirstSeen = DateTime.MinValue,
            LatestExpirationDate = DateTime.MinValue,
            ManagementURL = string.Empty,
            NonConsumablePurchases = new(),
            Entitlements = new(),
        };
    }
    public async partial Task<CustomerInfoDto?> GetCustomerInfo(CancellationToken cancellationToken)
    {
        return new()
        {
            ActiveSubscriptions = new(),
            AllPurchasedIdentifiers = new(),
            FirstSeen = DateTime.MinValue,
            LatestExpirationDate = DateTime.MinValue,
            ManagementURL = string.Empty,
            NonConsumablePurchases = new(),
            Entitlements = new(),
        };
    }

    internal static partial void EnableDebugLogs(bool enable)
    {
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
