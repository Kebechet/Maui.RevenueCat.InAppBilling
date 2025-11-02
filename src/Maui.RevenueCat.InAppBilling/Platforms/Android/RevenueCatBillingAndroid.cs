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
using IsNullOrEmpty.Extensions;

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
    public partial void Initialize(string apiKey, string appUserId)
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
                    .AppUserID(appUserId)
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
                return [];
            }

            return _cachedOfferingPackages.ToOfferingDtoList();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(GetOfferings)} was cancelled.");
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetOfferings)} didn't succeed.");
            return [];
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
            var purchaseError = (PurchaseErrorStatus)(ex?.PurchasesError?.Code.Code ?? 0);

            if (purchaseError != PurchaseErrorStatus.PurchaseCancelledError)
            {
                _logger.LogError(ex, "PurchasesErrorException");
            }

            return new PurchaseResultDto
            {
                ErrorStatus = purchaseError
            };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(PurchaseProduct)} was cancelled.");
            return new PurchaseResultDto
            {
                ErrorStatus = PurchaseErrorStatus.PurchaseCancelledError
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
            Transaction = purchaseSuccessInfo.StoreTransaction.ToStoreTransactionDto(),
            CustomerInfo = purchaseSuccessInfo.CustomerInfo.ToCustomerInfoDto()
        };
    }
    public async partial Task<List<string>> GetActiveSubscriptions(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return [];
            }

            if (customerInfo.ActiveSubscriptions.IsNullOrEmpty())
            {
                return [];
            }

            return customerInfo.ActiveSubscriptions.ToList(); ;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(GetActiveSubscriptions)} was cancelled.");
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve active subscriptions.");
            return [];
        }
    }
    public async partial Task<List<string>> GetAllPurchasedIdentifiers(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return [];
            }

            if (customerInfo.AllPurchasedProductIds.IsNullOrEmpty())
            {
                return [];
            }

            return customerInfo.AllPurchasedProductIds.ToList(); ;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(GetAllPurchasedIdentifiers)} was cancelled.");
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve all purchased identifiers.");
            return [];
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
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(GetPurchaseDateForProductIdentifier)} was cancelled.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve purchase date.");
            return null;
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
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(GetManagementSubscriptionUrl)} was cancelled.");
            return null;
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

            return customerInfo.ToCustomerInfoDto();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(Login)} was cancelled.");
            return null;
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

            return customerInfo.ToCustomerInfoDto();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(Logout)} was cancelled.");
            return null;
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

            return customerInfo.ToCustomerInfoDto();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(RestoreTransactions)} was cancelled.");
            return null;
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

            return customerInfo.ToCustomerInfoDto();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, $"{nameof(GetCustomerInfo)} was cancelled.");
            return null;
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
        Purchases.SharedInstance.SetEmail(email);
    }
    public partial void SetDisplayName(string name)
    {
        Purchases.SharedInstance.SetDisplayName(name);
    }
    public partial void SetPhoneNumber(string phone)
    {
        Purchases.SharedInstance.SetPhoneNumber(phone);
    }
    public partial void SetAttributes(IDictionary<string, string> attributes)
    {
        Purchases.SharedInstance.SetAttributes(attributes);
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
