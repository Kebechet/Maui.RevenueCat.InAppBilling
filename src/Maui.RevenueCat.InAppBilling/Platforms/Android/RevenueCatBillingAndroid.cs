using Com.Revenuecat.Purchases.Models;
using Com.Revenuecat.Purchases;
using Android.App;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Platforms.Android;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Models;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;
using Microsoft.Extensions.Logging;
using Maui.RevenueCat.InAppBilling.Enums;
using IsNullOrEmpty.Extensions;
using System.Runtime.CompilerServices;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private Purchases _purchases = default!;
    private Offerings? _cachedOfferingPackages = null;
    private static Activity? _currentActivityContext => Platform.CurrentActivity;

    public partial bool IsAnonymous() => Purchases.SharedInstance.IsAnonymous;
    public partial string GetAppUserId() => Purchases.SharedInstance.AppUserID;

    public async partial Task<CanMakePaymentsResultDto> CanMakePayments(CancellationToken cancellationToken)
    {
        if (_currentActivityContext is null)
        {
            return new() { Value = false };
        }

        try
        {
            return new() { Value = await _purchases.CanMakePaymentsAsync(_currentActivityContext, cancellationToken) };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

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

    public async partial Task<IntroEligibilityResultDto> CheckTrialOrIntroDiscountEligibility(List<string> identifiers, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("This method is iOS Only");
    }

    public async partial Task<OfferingsResultDto> GetOfferings(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!forceRefresh && _cachedOfferingPackages != null)
        {
            return new() { Value = _cachedOfferingPackages.ToOfferingDtoList() };
        }

        try
        {
            _cachedOfferingPackages = await Purchases.SharedInstance.GetOfferingsAsync(cancellationToken);
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
            var subscriptionReplacement = await GetSubscriptionReplacement(packageToBuy.Product, cancellationToken);

            // The lookup above can take a network round trip, long enough for the activity checked earlier to go away.
            var activity = _currentActivityContext
                ?? throw new InvalidOperationException("Android Current Activity went away before the purchase started.");
            purchaseSuccessInfo = await PurchasePackage(activity, packageToBuy, subscriptionReplacement, cancellationToken);
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

        var transaction = purchaseSuccessInfo.StoreTransaction.ToStoreTransactionDto();
        var customerInfo = purchaseSuccessInfo.CustomerInfo.ToCustomerInfoDto();

        var purchaseState = purchaseSuccessInfo.StoreTransaction.PurchaseState;

        if (purchaseState != PurchaseState.Purchased)
        {
            // The call succeeded but the transaction never reached Purchased. Only Pending means
            // "awaiting a slow payment method"; UnspecifiedState is Play Billing reporting that it
            // does not know the state - telling the user to check back later would be wrong.
            // This previously produced a result that was neither IsSuccess nor IsError, so callers
            // could not act on it at all.
            var errorStatus = purchaseState == PurchaseState.Pending
                ? PurchaseErrorStatus.PaymentPendingError
                : PurchaseErrorStatus.UnknownError;

            _logger.LogWarning(
                "{operationName} returned a transaction in state {transactionState}, reported as {errorStatus}.",
                nameof(PurchaseProduct),
                purchaseState?.ToString() ?? "unknown",
                errorStatus);

            return new PurchaseResultDto
            {
                Error = errorStatus,
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

    private async Task<PurchaseSuccessInfo> PurchasePackage(Activity activity, Package packageToBuy,
        SubscriptionReplacement? subscriptionReplacement, CancellationToken cancellationToken)
    {
        try
        {
            return await _purchases.PurchaseAsync(activity, packageToBuy, subscriptionReplacement, cancellationToken);
        }
        catch (PurchasesErrorException exception) when (subscriptionReplacement is not null
            && exception.PurchasesError?.IsNoExistingPurchaseError() == true)
        {
            // Google Play cannot replace a subscription it has no purchase of for this app and Google account: one
            // of another app in the same RevenueCat project, of another Google account, or one that has ended.
            // RevenueCat checks this before it opens the store sheet, so nothing has been bought yet.
            _logger.LogWarning(
                "Google Play has no purchase of {subscriptionToReplace} to replace, so {productToBuy} is bought without a replacement.",
                subscriptionReplacement.OldProductId,
                packageToBuy.Product.Id);
            return await _purchases.PurchaseAsync(activity, packageToBuy, subscriptionReplacement: null, cancellationToken);
        }
    }

    // Best effort, as in RevenueCat's own Paywalls: when the subscription to replace cannot be determined, the
    // purchase still goes ahead, without a replacement, as it did before the wrapper replaced subscriptions.
    private async Task<SubscriptionReplacement?> GetSubscriptionReplacement(IStoreProduct productToBuy, CancellationToken cancellationToken)
    {
        // A subscription from RevenueCat's Test Store is not a Google Play product and has nothing to replace.
        var googleProductToBuy = GoogleStoreProductKt.GetGoogleProduct(productToBuy);
        if (productToBuy.Type != ProductType.Subs || googleProductToBuy is null)
        {
            return null;
        }

        try
        {
            using var customerInfo = await GetCustomerInfoForSubscriptionReplacement(cancellationToken);
            var activePlayStoreSubscriptions = customerInfo.SubscriptionsByProductIdentifier.Values
                .Where(subscription => subscription.IsActive && subscription.Store == Store.PlayStore)
                .Select(subscription => subscription.ToActivePlayStoreSubscription())
                .ToList();

            if (!activePlayStoreSubscriptions.Any())
            {
                return null;
            }

            var productsOfActiveSubscriptions = await GetProductsOfThisApp(activePlayStoreSubscriptions, cancellationToken);
            var subscriptionToReplace = SubscriptionReplacementRules.FindSubscriptionToReplace(
                activePlayStoreSubscriptions,
                googleProductToBuy.ProductId,
                subscriptionIdsOfThisApp: productsOfActiveSubscriptions?.Select(product => product.ProductId).ToList());
            if (subscriptionToReplace is null)
            {
                return null;
            }

            var productToReplace = productsOfActiveSubscriptions?.FirstOrDefault(product =>
                SubscriptionReplacementRules.IsProductOf(subscriptionToReplace, product.ProductId, product.BasePlanId));

            // The defaults of RevenueCat's Paywalls, which also match an App Store subscription group: an upgrade
            // takes effect immediately, a downgrade at the next renewal.
            var replacementMode = SubscriptionReplacementRules.ChooseReplacementMode(
                isNewPlanPrepaid: productToBuy.IsDefaultOptionPrepaid(),
                oldComparablePrice: productToReplace?.GetComparablePrice(subscriptionToReplace.IsSandbox),
                newComparablePrice: productToBuy.GetComparablePrice(subscriptionToReplace.IsSandbox));

            _logger.LogDebug(
                "{productToBuy} replaces the active subscription {subscriptionToReplace} with replacement mode {replacementMode}.",
                productToBuy.Id,
                subscriptionToReplace.SubscriptionId,
                replacementMode);

            return new SubscriptionReplacement(subscriptionToReplace.SubscriptionId, replacementMode.ToStoreReplacementMode());
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Could not determine which active subscription {productToBuy} replaces, so it is bought without a replacement.",
                productToBuy.Id);
            return null;
        }
    }

    private async Task<CustomerInfo> GetCustomerInfoForSubscriptionReplacement(CancellationToken cancellationToken)
    {
        try
        {
            // NotStaleCachedOrFetched rather than the possibly stale cache RevenueCat's Paywalls read: a stale cache
            // can miss a subscription bought since, which would then not be replaced.
            return await _purchases.GetCustomerInfoAsync(CustomerInfoFetchPolicy.NotStaleCachedOrFetched, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Still better than no customer info, which would charge for both subscriptions. A subscription the
            // stale cache reports as active although it has ended is not replaced: RevenueCat finds no purchase of
            // it, and the purchase is retried without a replacement.
            _logger.LogWarning(exception, "Could not fetch customer info, so the active subscriptions are read from the cache.");
            return await _purchases.GetCustomerInfoAsync(CustomerInfoFetchPolicy.FromCacheOnly, cancellationToken);
        }
    }

    // Google Play lists only the products of this app. Null when the lookup fails: which of the subscriptions this
    // app sells, and the price of the plan to replace, are then unknown, but the subscription is still replaced.
    private async Task<List<GoogleStoreProduct>?> GetProductsOfThisApp(
        IEnumerable<ActivePlayStoreSubscription> activePlayStoreSubscriptions, CancellationToken cancellationToken)
    {
        var subscriptionIds = activePlayStoreSubscriptions.Select(subscription => subscription.SubscriptionId).Distinct().ToList();
        try
        {
            var products = await _purchases.GetSubscriptionProductsAsync(subscriptionIds, cancellationToken);
            return products.Select(GoogleStoreProductKt.GetGoogleProduct).OfType<GoogleStoreProduct>().ToList();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Could not read the products of the active subscriptions {subscriptionIds}, so which of them this app sells and their prices are unknown.",
                string.Join(", ", subscriptionIds));
            return null;
        }
    }

    public async partial Task<ProductIdentifiersResultDto> GetActiveSubscriptions(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(CustomerInfoFetchPolicy.CachedOrFetched, cancellationToken);
            if (customerInfo is null || customerInfo.ActiveSubscriptions.IsNullOrEmpty())
            {
                return new() { Value = [] };
            }

            return new() { Value = customerInfo.ActiveSubscriptions.ToList() };
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(CustomerInfoFetchPolicy.CachedOrFetched, cancellationToken);
            if (customerInfo is null || customerInfo.AllPurchasedProductIds.IsNullOrEmpty())
            {
                return new() { Value = [] };
            }

            return new() { Value = customerInfo.AllPurchasedProductIds.ToList() };
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(CustomerInfoFetchPolicy.CachedOrFetched, cancellationToken);
            if (customerInfo is null)
            {
                return new();
            }

            return new() { Value = customerInfo.GetPurchaseDateForProductId(productIdentifier).ToDateTime() };
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(CustomerInfoFetchPolicy.CachedOrFetched, cancellationToken);

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
            var customerInfo = await Purchases.SharedInstance.LogInAsync(appUserId, cancellationToken);

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
            var customerInfo = await Purchases.SharedInstance.LogOutAsync(cancellationToken);

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
            var customerInfo = await Purchases.SharedInstance.RestorePurchasesAsync(cancellationToken);

            return new() { Value = customerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }
    public async partial Task<CustomerInfoResultDto> GetCustomerInfo(CustomerInfoFetchPolicy fetchPolicy, CancellationToken cancellationToken)
    {
        try
        {
            var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(fetchPolicy, cancellationToken);

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
            // Matched before the code for the same reason as iOS: userCancelled is the
            // authoritative signal, and a missing PurchasesError or Code would otherwise
            // degrade a cancellation into UnknownError.
            PurchasesErrorException { UserCancelled: true } => PurchaseErrorStatus.PurchaseCancelledError,
            PurchasesErrorException purchasesErrorException =>
                purchasesErrorException.PurchasesError?.Code?.ToPurchaseErrorStatus() ?? PurchaseErrorStatus.UnknownError,
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

    public async partial Task<StorefrontResultDto> GetStorefrontCountryCode(CancellationToken cancellationToken)
    {
        try
        {
            using var callback = new DelegatingGetStorefrontCountryCodeCallback(cancellationToken);
            Purchases.SharedInstance.GetStorefrontCountryCode(callback);
            return new() { Value = await callback.Task };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
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
