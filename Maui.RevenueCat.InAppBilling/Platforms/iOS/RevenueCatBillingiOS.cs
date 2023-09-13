using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Platforms.iOS.Exceptions;
using Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
using Maui.RevenueCat.InAppBilling.Platforms.iOS.Models;
using Maui.RevenueCat.iOS;
using Microsoft.Extensions.Logging;
using Purchases = Maui.RevenueCat.iOS.RCPurchases;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private Purchases _purchases = default!;
    private List<RCPackage> _cachedOfferingPackages = new();

    public partial bool IsAnonymous() => Purchases.SharedPurchases.IsAnonymous;
    public partial string GetAppUserId() => Purchases.SharedPurchases.AppUserID;

    public partial void Initialize(string apiKey)
    {
        try
        {
            _purchases = Purchases.ConfigureWithAPIKey(apiKey);

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            // TODO - Ask user to verify logged in to Google and re-start app
            // Continuing is possible in some circumstances
            _logger.LogError(ex, "Initialization exception");
            throw;
        }
    }

    public async partial Task<Dictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibility(IList<string> identifiers, CancellationToken cancellationToken)
    {
        try
        {
            using var eligibilities = await _purchases.CheckTrialOrIntroDiscountEligibilityAsync(identifiers);
            if (eligibilities is null)
            {
                return new();
            }

            var _eligibilities = new Dictionary<string, IntroElegibilityStatus>();

            for (ulong i = 0; i < eligibilities.Count; i++)
            {
                _eligibilities.Add(eligibilities.Keys[i], eligibilities.Values[i].Status.Convert());
            }

            return _eligibilities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(CheckTrialOrIntroDiscountEligibility)} didn't succeed.");
            return new();
        }
    }

    public async partial Task<List<OfferingDto>> LoadOfferings(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!forceRefresh && !_cachedOfferingPackages.IsNullOrEmpty())
        {
            return _cachedOfferingPackages.ToOfferDtoList(); ;
        }

        try
        {
            using var offerings = await _purchases.GetOfferingsAsync();
            if (offerings is null)
            {
                return new();
            }

            using var currentOffering = offerings.Current;
            if (currentOffering is null)
            {
                return new();
            }

            _cachedOfferingPackages = currentOffering.AvailablePackages.ToList();

            return _cachedOfferingPackages.ToOfferDtoList(); ;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(LoadOfferings)} didn't succeed.");
            return new();
        }
    }
    public async partial Task<PurchaseResult> PurchaseProduct(string offeringIdentifier, CancellationToken cancellationToken)
    {
        if (!_isInitialized)
        {
            throw new Exception("RevenueCatBilling wasn't initialized");
        }

        var packageToBuy = _cachedOfferingPackages.FirstOrDefault(p => p.Identifier == offeringIdentifier);
        if (packageToBuy is null)
        {
            throw new Exception($"No offering/packege with identifier: {offeringIdentifier} found. Make sure you called LoadOfferings before.");
        }

        PurchaseSuccessInfo? purchaseSuccessInfo = null;

        try
        {
            purchaseSuccessInfo = await _purchases.PurchasePackageAsync(packageToBuy);
        }
        catch (PurchasesErrorException ex)
        {
            _logger.LogError(ex, "PurchasesErrorException");

            return new PurchaseResult
            {
                ErrorStatus = (PurchaseErrorStatus)(int)(ex?.PurchasesError?.Code ?? 0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in PurchaseProduct");

            return new PurchaseResult
            {
                ErrorStatus = PurchaseErrorStatus.UnknownError
            };
        }

        if (purchaseSuccessInfo is null || purchaseSuccessInfo.StoreTransaction.Sk1Transaction is null)
        {
            _logger.LogError($"{nameof(purchaseSuccessInfo)} is null.");

            return new PurchaseResult
            {
                ErrorStatus = PurchaseErrorStatus.UnknownError
            };
        }

        return new PurchaseResult
        {
            IsSuccess = purchaseSuccessInfo.StoreTransaction.Sk1Transaction.TransactionState == StoreKit.SKPaymentTransactionState.Purchased
        };
    }
    public async partial Task<List<string>> GetActiveSubscriptions(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await _purchases.GetCustomerInfoAsync();
            if (customerInfo is null)
            {
                return new();
            }

            if (customerInfo.ActiveSubscriptions.ToStringList().IsNullOrEmpty())
            {
                return new();
            }

            var activeSubscriptions = new List<string>();
            foreach (var activeSubscription in customerInfo.ActiveSubscriptions)
            {
                activeSubscriptions.Add(activeSubscription.ToString());
            }

            return activeSubscriptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve active subscriptions.");
            return new();
        }
    }
    public async partial Task<List<string>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await _purchases.GetCustomerInfoAsync();
            if (customerInfo is null)
            {
                return new();
            }

            if (customerInfo.AllPurchasedProductIdentifiers.ToStringList().IsNullOrEmpty())
            {
                return new();
            }

            return customerInfo.AllPurchasedProductIdentifiers.ToStringList(); ;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve all purchased identifiers.");
            return new();
        }
    }
    public async partial Task<DateTime?> GetPurchaseDateForProductIdentifier(string productIdentifier, CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await _purchases.GetCustomerInfoAsync();
            if (customerInfo is null)
            {
                return null;
            }

            return customerInfo.PurchaseDateForProductIdentifier(productIdentifier).ToDateTime();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve purchase date.");
            return new();
        }
    }
    public async partial Task<string?> GetManagementSubscriptionUrl(CancellationToken cancellationToken)
    {
        if (!_cachedManagementUrl.IsNullOrEmpty())
        {
            return _cachedManagementUrl;
        }

        try
        {
            using var customerInfo = await _purchases.GetCustomerInfoAsync();
            if (customerInfo is null || customerInfo.ManagementURL is null)
            {
                return string.Empty;
            }

            return customerInfo.ManagementURL.ToString()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve management url.");
            return null;
        }
    }
    public async partial Task<CustomerInfoDto?> Login(string appUserId, CancellationToken cancellationToken)
    {
        try
        {
            var loginResult = await Purchases.SharedPurchases.LoginAsync(appUserId, cancellationToken);
            var customerInfo = loginResult.CustomerInfo;

            return CustomerInfoToCustomerInfoDto(customerInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(Login)} failed.");
            return null;
        }
    }
    public async partial Task<CustomerInfoDto?> Logout(CancellationToken cancellationToken)
    {
        try
        {
            var customerInfo = await Purchases.SharedPurchases.LogOutAsync(cancellationToken);

            return CustomerInfoToCustomerInfoDto(customerInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(Logout)} failed.");
            return null;
        }
    }
    public async partial Task<CustomerInfoDto?> RestoreTransactions(CancellationToken cancellationToken)
    {
        try
        {
            var customerInfo = await Purchases.SharedPurchases.RestorePurchasesAsync(cancellationToken);

            return CustomerInfoToCustomerInfoDto(customerInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{RestoreTransactions} failed.");
            return null;
        }
    }

    internal static CustomerInfoDto CustomerInfoToCustomerInfoDto(RCCustomerInfo customerInfo)
    {
        return new CustomerInfoDto()
        {
            ActiveSubscriptions = customerInfo.ActiveSubscriptions.ToStringList(),
            AllPurchasedIdentifiers = customerInfo.AllPurchasedProductIdentifiers.ToStringList(),
            NonConsumablePurchases = customerInfo.NonConsumablePurchases.ToStringList(),
            FirstSeen = customerInfo.FirstSeen.ToDateTime(),
            LatestExpirationDate = customerInfo.LatestExpirationDate.ToDateTime(),
            ManagementURL = customerInfo?.ManagementURL?.ToString(),
        };
    }

    internal static partial void EnableDebugLogs(bool enable)
    {
        Purchases.DebugLogsEnabled = enable;
    }
}
