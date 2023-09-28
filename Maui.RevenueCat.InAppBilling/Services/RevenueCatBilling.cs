using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Microsoft.Extensions.Logging;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private readonly ILogger<RevenueCatBilling> _logger;

    private static bool _isInstanceCreated = false;
    private volatile bool _isInitialized = false;
    private string _cachedManagementUrl = string.Empty;

    public RevenueCatBilling(ILogger<RevenueCatBilling> logger)
    {
        if (_isInstanceCreated)
        {
            throw new InvalidOperationException($"You shouldn't create more instances of class RevenueCatBilling.");
        }

        _logger = logger;

        _isInstanceCreated = true;
    }

    public bool IsInitialized() => _isInitialized;
    public partial bool IsAnonymous();
    public partial string GetAppUserId();

    public partial void Initialize(string apiKey);
    public partial Task<Dictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibility(IList<string> identifiers, CancellationToken cancellationToken);
    public partial Task<List<OfferingDto>> LoadOfferings(bool forceRefresh, CancellationToken cancellationToken);
    public partial Task<PurchaseResult> PurchaseProduct(string offeringIdentifier, string packageIdentifier, CancellationToken cancellationToken);
    public partial Task<List<string>> GetActiveSubscriptions(CancellationToken cancellationToken);
    public partial Task<List<string>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken);
    public partial Task<DateTime?> GetPurchaseDateForProductIdentifier(string productIdentifier, CancellationToken cancellationToken);
    public partial Task<string?> GetManagementSubscriptionUrl(CancellationToken cancellationToken);
    public partial Task<CustomerInfoDto?> Login(string appUserId, CancellationToken cancellationToken);
    public partial Task<CustomerInfoDto?> Logout(CancellationToken cancellationToken);
    public partial Task<CustomerInfoDto?> RestoreTransactions(CancellationToken cancellationToken);

    internal static partial void EnableDebugLogs(bool enable);
}
