using Com.Revenuecat.Purchases.Models;
using Com.Revenuecat.Purchases;
using Android.App;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Models;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;
using Microsoft.Extensions.Logging;
using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private Purchases _purchases = default!;
    private Offerings? _cachedOfferingPackages = null;
    private static Activity? _currentActivityContext => Platform.CurrentActivity;

    public partial bool IsAnonymous() => Purchases.SharedInstance.IsAnonymous;
    public partial string GetAppUserId() => Purchases.SharedInstance.AppUserID;

    public partial void Initialize(string apiKey)
    {
        if (_currentActivityContext is null)
        {
            _logger.LogError("Android Activity is null");
            throw new Exception("You must call this code in App.xaml->OnStart");
        }

        try
        {
            _purchases = Purchases.Configure(
                new PurchasesConfiguration(
                    new PurchasesConfiguration.Builder(
                        _currentActivityContext,
                        apiKey)
                )
            );

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
        await Task.CompletedTask;
        throw new NotImplementedException("This method is iOS Only");
    }

    public async partial Task<List<OfferingDto>> GetOfferings(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!forceRefresh && _cachedOfferingPackages != null)
        {
            return _cachedOfferingPackages.ToOfferingDtoList();
        }

        try
        {
            _cachedOfferingPackages = await Purchases.SharedInstance.GetOfferingsAsync(cancellationToken);
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
            _logger.LogError($"To call {nameof(PurchaseProduct)} you firstly have to call Initialize method.");
            throw new Exception("RevenueCatBilling wasn't initialized");
        }

        if (_currentActivityContext is null)
        {
            throw new Exception("Android Current Activity can't be null.");
        }

        if (_cachedOfferingPackages is null)
        {
            throw new Exception("LoadOfferings must be called prior to purchasing a product.");
        }

        var offeringToBuy = _cachedOfferingPackages.GetOffering(packageToPurchase.OfferingIdentifier);
        if (offeringToBuy is null)
        {
            _logger.LogError("No offering with identifier: {offeringIdentifier} found. Make sure you called LoadOfferings before.", packageToPurchase.OfferingIdentifier);
            throw new Exception($"No offering with identifier: {packageToPurchase.OfferingIdentifier} found. Make sure you called LoadOfferings before.");
        }

        var packageToBuy = offeringToBuy.AvailablePackages.FirstOrDefault(p => p.Identifier == packageToPurchase.Identifier);
        if (packageToBuy is null)
        {
            _logger.LogError("No package with identifier: {packageIdentifier} found. Make sure you called LoadOfferings before.", packageToPurchase.Identifier);
            throw new Exception($"No offering with identifier: {packageToPurchase.Identifier} found. Make sure you called LoadOfferings before.");
        }

        PurchaseSuccessInfo? purchaseSuccessInfo = null;

        try
        {
            purchaseSuccessInfo = await _purchases.PurchaseAsync(_currentActivityContext, packageToBuy, cancellationToken);
        }
        catch (PurchasesErrorException ex)
        {
            _logger.LogError(ex, "PurchasesErrorException");

            return new PurchaseResultDto
            {
                ErrorStatus = (PurchaseErrorStatus)(ex?.PurchasesError?.Code.Code ?? 0)
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

        if (purchaseSuccessInfo is null)
        {
            _logger.LogError($"{nameof(purchaseSuccessInfo)} is null.");

            return new PurchaseResultDto
            {
                ErrorStatus = PurchaseErrorStatus.UnknownError
            };
        }

        return new PurchaseResultDto
        {
            IsSuccess = purchaseSuccessInfo.StoreTransaction.PurchaseState == PurchaseState.Purchased,
            CustomerInfo= CustomerInfoToCustomerInfoDto(purchaseSuccessInfo.CustomerInfo)
        };
    }
    public async partial Task<List<string>> GetActiveSubscriptions(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return new();
            }

            if (customerInfo.ActiveSubscriptions.IsNullOrEmpty())
            {
                return new();
            }

            return customerInfo.ActiveSubscriptions.ToList(); ;
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return new();
            }

            if (customerInfo.AllPurchasedProductIds.IsNullOrEmpty())
            {
                return new();
            }

            return customerInfo.AllPurchasedProductIds.ToList(); ;
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return null;
            }

            return customerInfo.GetPurchaseDateForProductId(productIdentifier).ToDateTime();
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null || customerInfo.ManagementURL is null)
            {
                return null;
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
            var customerInfo = await Purchases.SharedInstance.LogInAsync(appUserId, cancellationToken);

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
            var customerInfo = await Purchases.SharedInstance.LogOutAsync(cancellationToken);

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
            var customerInfo = await Purchases.SharedInstance.RestorePurchasesAsync(cancellationToken);

            return CustomerInfoToCustomerInfoDto(customerInfo);
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
            var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);

            return CustomerInfoToCustomerInfoDto(customerInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetCustomerInfo)} failed.");
            return null;
        }
    }

    internal static CustomerInfoDto CustomerInfoToCustomerInfoDto(CustomerInfo customerInfo)
    {
        return new CustomerInfoDto()
        {
            ActiveSubscriptions = customerInfo.ActiveSubscriptions.ToList(),
            AllPurchasedIdentifiers = customerInfo.AllPurchasedProductIds.ToList(),
            //Google Play Store does not provide an option to mark IAPs as consumable or non-consumable
            //https://www.revenuecat.com/docs/non-subscriptions
            NonConsumablePurchases = new(),
            FirstSeen = customerInfo.FirstSeen.ToDateTime(),
            LatestExpirationDate = customerInfo.LatestExpirationDate.ToDateTime(),
            ManagementURL = customerInfo.ManagementURL?.ToString(),
            Entitlements = customerInfo.Entitlements.ToEntitlementInfoDtoList(),
        };
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
