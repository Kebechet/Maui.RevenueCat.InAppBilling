using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public partial bool IsAnonymous() => true;
    public partial string GetAppUserId() => string.Empty;
    public async partial Task<CanMakePaymentsResultDto> CanMakePayments(CancellationToken cancellationToken)
    {
        return new() { Value = false };
    }

    public partial void Initialize(string apiKey)
    {
        _isInitialized = true;
    }
    public partial void Initialize(string apiKey, string appUserId)
    {
        _isInitialized = true;
    }
    public async partial Task<IntroEligibilityResultDto> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken)
    {
        return new() { Value = [] };
    }

    public async partial Task<OfferingsResultDto> GetOfferings(bool forceRefresh, CancellationToken cancellationToken)
    {
        return new() { Value = [] };
    }
    public async partial Task<PurchaseResultDto> PurchaseProduct(PackageDto packageToPurchase, CancellationToken cancellationToken)
    {
        return new() { Value = CreateEmptyCustomerInfo() };
    }
    public async partial Task<ProductIdentifiersResultDto> GetActiveSubscriptions(CancellationToken cancellationToken)
    {
        return new() { Value = [] };
    }
    public async partial Task<ProductIdentifiersResultDto> GetAllPurchasedIdentifiers(CancellationToken cancellationToken)
    {
        return new() { Value = [] };
    }
    public async partial Task<PurchaseDateResultDto> GetPurchaseDateForProductIdentifier(string productIdentifier, CancellationToken cancellationToken)
    {
        return new();
    }
    public async partial Task<ManagementUrlResultDto> GetManagementSubscriptionUrl(CancellationToken cancellationToken)
    {
        return new();
    }
    public async partial Task<CustomerInfoResultDto> Login(string appUserId, CancellationToken cancellationToken)
    {
        return new() { Value = CreateEmptyCustomerInfo() };
    }
    public async partial Task<CustomerInfoResultDto> Logout(CancellationToken cancellationToken)
    {
        return new() { Value = CreateEmptyCustomerInfo() };
    }
    public async partial Task<CustomerInfoResultDto> RestoreTransactions(CancellationToken cancellationToken)
    {
        return new() { Value = CreateEmptyCustomerInfo() };
    }
    public async partial Task<CustomerInfoResultDto> GetCustomerInfo(CancellationToken cancellationToken)
    {
        return new() { Value = CreateEmptyCustomerInfo() };
    }

    private static CustomerInfoDto CreateEmptyCustomerInfo() => new()
    {
        ActiveSubscriptions = [],
        AllPurchasedIdentifiers = [],
        FirstSeen = DateTime.MinValue,
        LatestExpirationDate = DateTime.MinValue,
        ManagementUrl = null,
        NonConsumablePurchases = [],
        Entitlements = [],
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

    public partial Task<StorefrontResultDto> GetStorefrontCountryCode(CancellationToken cancellationToken)
    {
        return Task.FromResult(new StorefrontResultDto { Value = string.Empty });
    }

    internal static partial void EnableDebugLogs(bool enable)
    {
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
