using IsNullOrEmpty.Extensions;
using System.Runtime.CompilerServices;
using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Platforms.iOS.Exceptions;
using Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
using Maui.RevenueCat.InAppBilling.Platforms.iOS.Models;
using Maui.RevenueCat.iOS;
using Maui.RevenueCat.Platforms.iOS.Extensions;
using Microsoft.Extensions.Logging;
using Types.Result;
using Purchases = Maui.RevenueCat.iOS.RCPurchases;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private Purchases _purchases = default!;
    private RCOfferings? _cachedOfferingPackages = null;

    public partial bool IsAnonymous() => Purchases.SharedPurchases.IsAnonymous;
    public partial string GetAppUserId() => Purchases.SharedPurchases.AppUserID;

    public partial Task<CanMakePaymentsResultDto> CanMakePayments(CancellationToken cancellationToken)
    {
        return Task.FromResult(new CanMakePaymentsResultDto { Value = Purchases.CanMakePayments });
    }

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

    public async partial Task<IntroEligibilityResultDto> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken)
    {
        try
        {
            using var eligibilities = await _purchases.CheckTrialOrIntroDiscountEligibilityAsync(identifiers, cancellationToken);
            if (eligibilities.IsNullOrEmpty())
            {
                return new() { Value = [] };
            }

            var eligibilitiesResult = new Dictionary<string, IntroElegibilityStatus>();

            for (ulong i = 0; i < eligibilities.Count; i++)
            {
                eligibilitiesResult.Add(eligibilities.Keys[i], eligibilities.Values[i].Status.Convert());
            }

            return new() { Value = eligibilitiesResult };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    public async partial Task<OfferingsResultDto> GetOfferings(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!forceRefresh && _cachedOfferingPackages != null)
        {
            return new() { Value = _cachedOfferingPackages.ToOfferingDtoList() };
        }

        try
        {
            _cachedOfferingPackages = await _purchases.GetOfferingsAsync(cancellationToken);
            if (_cachedOfferingPackages is null)
            {
                return new() { Value = [] };
            }

            return new() { Value = _cachedOfferingPackages.ToOfferingDtoList() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
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
            purchaseSuccessInfo = await _purchases.PurchasePackageAsync(packageToBuy, cancellationToken);
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }

        if (purchaseSuccessInfo is null)
        {
            var missingInfo = new InvalidOperationException($"{nameof(purchaseSuccessInfo)} is null");
            return new() { Error = LogAndMapError(missingInfo), ErrorException = missingInfo };
        }

        var isPurchased = purchaseSuccessInfo.StoreTransaction.Sk1Transaction is not null
            ? purchaseSuccessInfo.StoreTransaction.Sk1Transaction.TransactionState == StoreKit.SKPaymentTransactionState.Purchased
            : !purchaseSuccessInfo.StoreTransaction.TransactionIdentifier.IsNullOrEmpty();

        var transaction = purchaseSuccessInfo.StoreTransaction.ToStoreTransactionDto();
        var customerInfo = purchaseSuccessInfo.CustomerInfo.ToCustomerInfoDto();

        if (!isPurchased)
        {
            // The call succeeded but the transaction never reached Purchased, which on StoreKit
            // means it is deferred / awaiting approval. This previously produced a result that was
            // neither IsSuccess nor IsError, so callers could not act on it at all.
            _logger.LogWarning("{operationName} returned a transaction that is not in the purchased state.", nameof(PurchaseProduct));

            return new PurchaseResultDto
            {
                Error = PurchaseErrorStatus.PaymentPendingError,
                Transaction = transaction,
                Value = customerInfo
            };
        }

        return new PurchaseResultDto
        {
            Transaction = transaction,
            Value = customerInfo
        };
    }
    public async partial Task<ProductIdentifiersResultDto> GetActiveSubscriptions(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await _purchases.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null || customerInfo.ActiveSubscriptions.ToStringList().IsNullOrEmpty())
            {
                return new() { Value = [] };
            }

            var activeSubscriptions = new List<string>();
            foreach (var activeSubscription in customerInfo.ActiveSubscriptions)
            {
                activeSubscriptions.Add(activeSubscription.ToString());
            }

            return new() { Value = activeSubscriptions };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }
    public async partial Task<ProductIdentifiersResultDto> GetAllPurchasedIdentifiers(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await _purchases.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return new() { Value = [] };
            }

            return new() { Value = customerInfo.AllPurchasedProductIdentifiers.ToStringList() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }
    public async partial Task<PurchaseDateResultDto> GetPurchaseDateForProductIdentifier(string productIdentifier, CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await _purchases.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return new();
            }

            return new() { Value = customerInfo.PurchaseDateForProductIdentifier(productIdentifier).ToDateTime() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }
    public async partial Task<ManagementUrlResultDto> GetManagementSubscriptionUrl(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await _purchases.GetCustomerInfoAsync(cancellationToken);

            return new() { Value = customerInfo?.ManagementURL?.ToString() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }
    public async partial Task<CustomerInfoResultDto> Login(string appUserId, CancellationToken cancellationToken)
    {
        try
        {
            var loginResult = await Purchases.SharedPurchases.LoginAsync(appUserId, cancellationToken);
            var customerInfo = loginResult.CustomerInfo;

            return new() { Value = customerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }
    public async partial Task<CustomerInfoResultDto> Logout(CancellationToken cancellationToken)
    {
        try
        {
            var customerInfo = await Purchases.SharedPurchases.LogOutAsync(cancellationToken);

            return new() { Value = customerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }
    public async partial Task<CustomerInfoResultDto> RestoreTransactions(CancellationToken cancellationToken)
    {
        try
        {
            var customerInfo = await Purchases.SharedPurchases.RestorePurchasesAsync(cancellationToken);

            return new() { Value = customerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }
    public async partial Task<CustomerInfoResultDto> GetCustomerInfo(CancellationToken cancellationToken)
    {
        try
        {
            var customerInfo = await Purchases.SharedPurchases.GetCustomerInfoAsync(cancellationToken);

            return new() { Value = customerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }
    private PurchaseErrorStatus LogAndMapError(Exception exception, [CallerMemberName] string operationName = "")
    {
        var errorStatus = exception switch
        {
            PurchasesErrorException purchasesErrorException => purchasesErrorException.PurchasesErrorCode.ToPurchaseErrorStatus(),
            OperationCanceledException => PurchaseErrorStatus.PurchaseCancelledError,
            _ => PurchaseErrorStatus.UnknownError,
        };

        if (errorStatus == PurchaseErrorStatus.PurchaseCancelledError)
        {
            _logger.LogDebug(exception, "{operationName} was cancelled.", operationName);
        }
        else
        {
            _logger.LogError(exception, "{operationName} failed.", operationName);
        }

        return errorStatus;
    }


    // Subscriber Attributes
    public partial void SetEmail(string email)
    {
        Purchases.SharedPurchases.Attribution.SetEmail(email);
    }
    public partial void SetDisplayName(string name)
    {
        Purchases.SharedPurchases.Attribution.SetDisplayName(name);
    }
    public partial void SetPhoneNumber(string phone)
    {
        Purchases.SharedPurchases.Attribution.SetPhoneNumber(phone);
    }
    public partial void SetAttributes(IDictionary<string, string> attributes)
    {
        var nsAttributes = attributes.ToNSDictionary();
        Purchases.SharedPurchases.Attribution.SetAttributes(nsAttributes);
    }

    public partial Task<StorefrontResultDto> GetStorefrontCountryCode(CancellationToken cancellationToken)
    {
        // StoreFrontCountryCode is a sync property on RCPurchases populated when the
        // user's storefront is first observed. May be null before the SDK has talked
        // to StoreKit at least once — defaults to string.Empty in that window.
        // StoreKit reports the storefront as ISO alpha-3 ("USA"); the interface contract
        // (and the Android implementation) use alpha-2, so normalize it.
        var storefrontCountryCode = _purchases.StoreFrontCountryCode ?? string.Empty;
        return Task.FromResult(new StorefrontResultDto { Value = storefrontCountryCode.ToIsoAlpha2CountryCode() });
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
