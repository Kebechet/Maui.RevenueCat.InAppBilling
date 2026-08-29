using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public partial bool IsAnonymous() => true;
    public partial string GetAppUserId() => string.Empty;
    public async partial Task<bool> CanMakePayments(CancellationToken cancellationToken)
    {
        return false;
    }

    public partial void Initialize(string apiKey)
    {
        _isInitialized = true;
    }
    public partial void Initialize(string apiKey, string appUserId)
    {
        _isInitialized = true;
    }
    public async partial Task<Dictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken)
    {
        return [];
    }

    public async partial Task<List<OfferingDto>> GetOfferings(bool forceRefresh, CancellationToken cancellationToken)
    {
        return [];
    }
    public async partial Task<PurchaseResultDto> PurchaseProduct(PackageDto packageToPurchase, CancellationToken cancellationToken)
    {
        return new();
    }
    public async partial Task<List<string>> GetActiveSubscriptions(CancellationToken cancellationToken)
    {
        return [];
    }
    public async partial Task<List<string>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken)
    {
        return [];
    }
    public async partial Task<DateTime?> GetPurchaseDateForProductIdentifier(string productIdentifier, CancellationToken cancellationToken)
    {
        return DateTime.MinValue;
    }
    public async partial Task<ManagementUrlResultDto> GetManagementSubscriptionUrl(CancellationToken cancellationToken)
    {
        return new()
        {
            IsSuccess = true,
        };
    }
    public async partial Task<CustomerInfoResultDto> Login(string appUserId, CancellationToken cancellationToken)
    {
        return CreateEmptyCustomerInfoResult();
    }
    public async partial Task<CustomerInfoResultDto> Logout(CancellationToken cancellationToken)
    {
        return CreateEmptyCustomerInfoResult();
    }
    public async partial Task<CustomerInfoResultDto> RestoreTransactions(CancellationToken cancellationToken)
    {
        return CreateEmptyCustomerInfoResult();
    }
    public async partial Task<CustomerInfoResultDto> GetCustomerInfo(CancellationToken cancellationToken)
    {
        return CreateEmptyCustomerInfoResult();
    }

    private static CustomerInfoResultDto CreateEmptyCustomerInfoResult() => new()
    {
        IsSuccess = true,
        CustomerInfo = new()
        {
            ActiveSubscriptions = [],
            AllPurchasedIdentifiers = [],
            FirstSeen = DateTime.MinValue,
            LatestExpirationDate = DateTime.MinValue,
            ManagementUrl = string.Empty,
            NonConsumablePurchases = [],
            Entitlements = [],
        },
    };

    // Subscriber Attributes
    public partial void SetEmail(string email)
    {
    }
    public partial void SetDisplayName(string name)
    {
    }
    public partial void SetPhoneNumber(string phone)
    {
    }
    public partial void SetAttributes(IDictionary<string, string> attributes)
    {
    }

    public partial Task<string> GetStorefrontCountryCode(CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty);
    }

    internal static partial void EnableDebugLogs(bool enable)
    {
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
