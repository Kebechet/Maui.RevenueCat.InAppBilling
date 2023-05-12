using Com.Revenuecat.Purchases.Models;
using Com.Revenuecat.Purchases;
using Android.App;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Models;
using Maui.RevenueCat.InAppBilling.Platforms.Android.Exceptions;

namespace Maui.RevenueCat.InAppBilling.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private Purchases _purchases = default!;
    private List<Package> _cachedOfferingPackages = new();
    private static Activity? _currentActivityContext => Platform.CurrentActivity;

    public partial bool IsAnonymous() => Purchases.SharedInstance.IsAnonymous;
    public partial string GetAppUserId() => Purchases.SharedInstance.AppUserID;

    public partial void Initialize(string apiKey)
    {
        if (_currentActivityContext is null)
        {
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
        catch (PurchasesErrorException ex)
        {
            var description = ex.PurchasesError?.Code.Description;
            var diagnosis = ex.PurchasesError?.UnderlyingErrorMessage;
            //var msg = new Show_Dialog();
            //if(description.Contains("problem with the store"))
            //	description = S.StoreProblemInDetail;       // Ask user to verify logged in to Google and re-start app
            //await msg.ShowDialogAsync(S.Warning, description);

            // Continuing is possible in some circumstances
            return;
        }
        catch (Exception ex)
        {
            //MyUtil.WriteLogFile(S.Exception, ex.ToString());
            throw new Exception("In InAppPurchases.InitialiseRevenueCatAsync " + ex.ToString(), ex.InnerException);
        }
    }
    public async partial Task<List<OfferingDto>> LoadOfferings(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!forceRefresh && !_cachedOfferingPackages.IsNullOrEmpty())
        {
            return _cachedOfferingPackages.ToOfferDtoList(); ;
        }

        using var offerings = await Purchases.SharedInstance.GetOfferingsAsync(cancellationToken);
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
    public async partial Task<bool> PurchaseProduct(string offeringIdentifier, CancellationToken cancellationToken)
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

        if(_currentActivityContext is null)
        {
            throw new Exception("Android Current Activity can't be null.");
        }

        PurchaseSuccessInfo? purchaseSuccessInfo = null;

        try
        {
            purchaseSuccessInfo = await _purchases.PurchasePackageAsync(_currentActivityContext, packageToBuy, cancellationToken);
        }
        catch (PurchasesErrorException)
        {
            //TODO
            //error code
        }
        catch (Exception ex)
        {
            //MyUtil.WriteLogFile(S.Exception, ex.ToString());
            throw new Exception("In InAppPurchases.PurchaseProduct " + ex.ToString(), ex.InnerException);
        }

        if (purchaseSuccessInfo is null)
        {
            return false;
        }

        return purchaseSuccessInfo.StoreTransaction.PurchaseState == PurchaseState.Purchased;
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
            //MyUtil.WriteLogFile(S.Exception, ex.ToString());
            throw new Exception("In InAppPurchases.ActiveSubscriptionsAsync " + ex.ToString(), ex.InnerException);
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

            if (customerInfo.AllPurchasedSkus.IsNullOrEmpty())
            {
                return new();
            }

            return customerInfo.AllPurchasedSkus.ToList(); ;
        }
        catch (Exception ex)
        {
            //MyUtil.WriteLogFile(S.Exception, ex.ToString());
            throw new Exception("In InAppPurchases.AllPurchasedProductIdentifiersAsync " + ex.ToString(), ex.InnerException);
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

            return customerInfo.GetPurchaseDateForSku(productIdentifier).ToDateTime();
        }
        catch (Exception ex)
        {
            //MyUtil.WriteLogFile(S.Exception, ex.ToString());
            throw new Exception("In InAppPurchases.PurchaseDateForProductIdentifierAsync " + ex.ToString(), ex.InnerException);
        }
    }
    public async partial Task<string> GetManagementSubscriptionUrl(CancellationToken cancellationToken)
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
                return string.Empty;
            }

            return customerInfo.ManagementURL.ToString()!;
        }
        catch (Exception ex)
        {
            //MyUtil.WriteLogFile(S.Exception, ex.ToString());
            throw new Exception("In InAppPurchases.ManagementUrlAsync " + ex.ToString(), ex.InnerException);
        }
    }
    public async partial Task<CustomerInfoDto> Login(string appUserId, CancellationToken cancellationToken)
    {
        var customerInfo = await Purchases.SharedInstance.LogInAsync(appUserId, cancellationToken);

        return new CustomerInfoDto()
        {
            ActiveSubscriptions = customerInfo.ActiveSubscriptions.ToList(),
            AllPurchasedIdentifiers = customerInfo.AllPurchasedSkus.ToList(),
            NonConsumablePurchases = customerInfo.PurchasedNonSubscriptionSkus.ToList(),
            FirstSeen = customerInfo.FirstSeen.ToDateTime(),
            LatestExpirationDate = customerInfo.LatestExpirationDate.ToDateTime(),
            ManagementURL = customerInfo.ManagementURL.ToString(),
        };
    }
    public async partial Task<CustomerInfoDto> Logout(CancellationToken cancellationToken)
    {
        var customerInfo = await Purchases.SharedInstance.LogOutAsync(cancellationToken);

        return new CustomerInfoDto()
        {
            ActiveSubscriptions = customerInfo.ActiveSubscriptions.ToList(),
            AllPurchasedIdentifiers = customerInfo.AllPurchasedSkus.ToList(),
            NonConsumablePurchases = customerInfo.PurchasedNonSubscriptionSkus.ToList(),
            FirstSeen = customerInfo.FirstSeen.ToDateTime(),
            LatestExpirationDate = customerInfo.LatestExpirationDate.ToDateTime(),
            ManagementURL = customerInfo.ManagementURL.ToString(),
        };
    }
    public async partial Task<CustomerInfoDto> RestoreTransactions(CancellationToken cancellationToken)
    {
        var customerInfo = await Purchases.SharedInstance.RestorePurchasesAsync(cancellationToken);

        return new CustomerInfoDto()
        {
            ActiveSubscriptions = customerInfo.ActiveSubscriptions.ToList(),
            AllPurchasedIdentifiers = customerInfo.AllPurchasedSkus.ToList(),
            NonConsumablePurchases = customerInfo.PurchasedNonSubscriptionSkus.ToList(),
            FirstSeen = customerInfo.FirstSeen.ToDateTime(),
            LatestExpirationDate = customerInfo.LatestExpirationDate.ToDateTime(),
            ManagementURL = customerInfo.ManagementURL.ToString(),
        };
    }

    internal static partial void EnableDebugLogs(bool enable)
    {
        Purchases.DebugLogsEnabled = enable;
    }
}
