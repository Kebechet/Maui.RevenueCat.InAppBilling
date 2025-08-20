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
    private RCOfferings? _cachedOfferingPackages = null;

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

    public partial void Initialize(string apiKey, string appUserId)
    {
        try
        {
            _purchases = Purchases.ConfigureWithAPIKey(apiKey, appUserId);

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

    public async partial Task<Dictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken)
    {
        try
        {
            using var eligibilities = await _purchases.CheckTrialOrIntroDiscountEligibilityAsync(identifiers);
            if (eligibilities.IsNullOrEmpty())
            {
                return new();
            }

            var eligibilitiesResult = new Dictionary<string, IntroElegibilityStatus>();

            for (ulong i = 0; i < eligibilities.Count; i++)
            {
                eligibilitiesResult.Add(eligibilities.Keys[i], eligibilities.Values[i].Status.Convert());
            }

            return eligibilitiesResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(CheckTrialOrIntroDiscountEligibility)} didn't succeed.");
            return new();
        }
    }

    public async partial Task<List<OfferingDto>> GetOfferings(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!forceRefresh && _cachedOfferingPackages != null)
        {
            return _cachedOfferingPackages.ToOfferingDtoList();
        }

        try
        {
            _cachedOfferingPackages = await _purchases.GetOfferingsAsync();
            if (_cachedOfferingPackages is null)
            {
                return new();
            }

            return _cachedOfferingPackages.ToOfferingDtoList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetOfferings)} didn't succeed.");
            return new();
        }
    }
    public async partial Task<PurchaseResultDto> PurchaseProduct(PackageDto packageToPurchase, CancellationToken cancellationToken)
    {
        if (!_isInitialized)
        {
            throw new Exception("RevenueCatBilling wasn't initialized");
        }

        if (_cachedOfferingPackages is null)
        {
            throw new Exception("LoadOfferings must be called prior to purchasing a product.");
        }

        var offeringToBuy = _cachedOfferingPackages.OfferingWithIdentifier(packageToPurchase.OfferingIdentifier);
        if (offeringToBuy is null)
        {
            throw new Exception($"No offering with identifier: {packageToPurchase.OfferingIdentifier} found. Make sure you called LoadOfferings before.");
        }

        var packageToBuy = offeringToBuy.AvailablePackages.FirstOrDefault(p => p.Identifier == packageToPurchase.Identifier);
        if (packageToBuy is null)
        {
            throw new Exception($"No package with identifier: {packageToPurchase.Identifier} found. Make sure you called LoadOfferings before.");
        }

        PurchaseSuccessInfo? purchaseSuccessInfo = null;

        try
        {
            purchaseSuccessInfo = await _purchases.PurchasePackageAsync(packageToBuy);
        }
        catch (PurchasesErrorException ex)
        {
            var purchaseError = (PurchaseErrorStatus)(int)(ex?.PurchasesError?.Code ?? 0);

            if (purchaseError != PurchaseErrorStatus.PurchaseCancelledError)
            {
                _logger.LogError(ex, "PurchasesErrorException");
            }

            return new PurchaseResultDto
            {
                ErrorStatus = purchaseError
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in PurchaseProduct");

            return new PurchaseResultDto
            {
                ErrorStatus = PurchaseErrorStatus.UnknownError
            };
        }

        if (purchaseSuccessInfo is null || purchaseSuccessInfo.StoreTransaction.Sk1Transaction is null)
        {
            _logger.LogError($"{nameof(purchaseSuccessInfo)} is null.");

            return new PurchaseResultDto
            {
                ErrorStatus = PurchaseErrorStatus.UnknownError
            };
        }

        return new PurchaseResultDto
        {
            IsSuccess = purchaseSuccessInfo.StoreTransaction.Sk1Transaction.TransactionState == StoreKit.SKPaymentTransactionState.Purchased,
            CustomerInfo = purchaseSuccessInfo.CustomerInfo.ToCustomerInfoDto()
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

            return customerInfo.ToCustomerInfoDto();
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

            return customerInfo.ToCustomerInfoDto();
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

            return customerInfo.ToCustomerInfoDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(RestoreTransactions)} failed.");
            return null;
        }
    }
    public async partial Task<CustomerInfoDto?> GetCustomerInfo(CancellationToken cancellationToken)
    {
        try
        {
            var customerInfo = await Purchases.SharedPurchases.GetCustomerInfoAsync(cancellationToken);

            return customerInfo.ToCustomerInfoDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetCustomerInfo)} failed.");
            return null;
        }
    }

    // Subscriber Attributes
    public partial void SetEmail(string email)
    {
        Purchases.SharedPurchases.SetEmail(email);
    }

    public partial void SetDisplayName(string name)
    {
        Purchases.SharedPurchases.SetDisplayName(name);
    }

    public partial void SetPhoneNumber(string phone)
    {
        Purchases.SharedPurchases.SetPhoneNumber(phone);
    }

    public partial void SetAttributes(IDictionary<string, string> attributes)
    {
        Purchases.SharedPurchases.SetAttributes(attributes);
    }

    internal static partial void EnableDebugLogs(bool enable)
    {
        if (!enable)
        {
            return;
        }

        Purchases.LogLevel = LogLevel.Debug.ToRCLogLevel();
    }
}
