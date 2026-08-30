using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Microsoft.Extensions.Logging;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private readonly ILogger<RevenueCatBilling> _logger;

    private static bool _isInstanceCreated = false;
    private volatile bool _isInitialized = false;

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
    public partial Task<DataResult<bool, PurchaseErrorStatus>> CanMakePayments(CancellationToken cancellationToken);

    public partial void Initialize(string apiKey);
    public partial void Initialize(string apiKey, string appUserId);

    public partial Task<DataResult<Dictionary<string, IntroElegibilityStatus>, PurchaseErrorStatus>> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken);
    public partial Task<DataResult<List<OfferingDto>, PurchaseErrorStatus>> GetOfferings(bool forceRefresh, CancellationToken cancellationToken);
    public partial Task<PurchaseResultDto> PurchaseProduct(PackageDto packageToPurchase, CancellationToken cancellationToken);
    public partial Task<DataResult<List<string>, PurchaseErrorStatus>> GetActiveSubscriptions(CancellationToken cancellationToken);
    public partial Task<DataResult<List<string>, PurchaseErrorStatus>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken);
    public partial Task<DataResult<DateTime?, PurchaseErrorStatus>> GetPurchaseDateForProductIdentifier(string productIdentifier, CancellationToken cancellationToken);
    public partial Task<DataResult<string, PurchaseErrorStatus>> GetManagementSubscriptionUrl(CancellationToken cancellationToken);
    public partial Task<DataResult<CustomerInfoDto, PurchaseErrorStatus>> Login(string appUserId, CancellationToken cancellationToken);
    public partial Task<DataResult<CustomerInfoDto, PurchaseErrorStatus>> Logout(CancellationToken cancellationToken);
    public partial Task<DataResult<CustomerInfoDto, PurchaseErrorStatus>> RestoreTransactions(CancellationToken cancellationToken);
    public partial Task<DataResult<CustomerInfoDto, PurchaseErrorStatus>> GetCustomerInfo(CancellationToken cancellationToken);
    public partial Task<DataResult<string, PurchaseErrorStatus>> GetStorefrontCountryCode(CancellationToken cancellationToken);

    // Subscriber Attributes
    public partial void SetEmail(string email);
    public partial void SetDisplayName(string name);
    public partial void SetPhoneNumber(string phone);
    public partial void SetAttributes(IDictionary<string, string> attributes);

    internal static partial void EnableDebugLogs(bool enable);
}
