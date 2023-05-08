using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private static bool _isInstanceCreated = false;
    private volatile bool _isInitialized = false;

    private string _cachedManagementUrl = string.Empty;

    public static partial void EnableDebugLogs(bool enable);

    public RevenueCatBilling()
    {
        if (_isInstanceCreated)
        {
            throw new InvalidOperationException($"You shouldn't create more instances of class RevenueCatBilling.");
        }

        _isInstanceCreated = true;
    }

    public bool IsInitialized()
    {
        return _isInitialized;
    }

    public partial void Initialize(string apiKey);
    public partial Task<List<OfferingDto>> LoadOfferings(bool forceRefresh = false);
    public partial Task<bool> PurchaseProduct(string offeringIdentifier);
    public partial Task<List<string>> GetActiveSubscriptions();
    public partial Task<List<string>> GetAllPurchasedIdentifiers();
    public partial Task<DateTime?> GetPurchaseDateForProductIdentifier(string productIdentifier);
    public partial Task<string> GetManagementSubscriptionUrl();
}
