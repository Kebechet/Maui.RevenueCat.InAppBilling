using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

    public static partial void EnableDebugLogs(bool enable)
    {
    }

    public partial void Initialize(string apiKey)
    {
        _isInitialized = true;
    }

    public async partial Task<List<OfferingDto>> LoadOfferings(bool forceRefresh)
    {
        return new();
    }
    public async partial Task<bool> PurchaseProduct(string offeringIdentifier)
    {
        return true;
    }
    public async partial Task<List<string>> GetActiveSubscriptions()
    {
        return new();
    }
    public async partial Task<List<string>> GetAllPurchasedIdentifiers()
    {
        return new();
    }
    public async partial Task<DateTime?> GetPurchaseDateForProductIdentifier(string productIdentifier)
    {
        return DateTime.MinValue;
    }
    public async partial Task<string> GetManagementSubscriptionUrl()
    {
        return string.Empty;
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
