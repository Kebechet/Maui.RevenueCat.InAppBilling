using Foundation;
using Maui.RevenueCat.InAppBilling.Platforms.iOS.Exceptions;
using Maui.RevenueCat.InAppBilling.Platforms.iOS.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class PurchasesExtensions
{
    internal static Task<LoginResult> LoginAsync(this RCPurchases purchases, string appUserId,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<LoginResult>();
        cancellationToken.Register(() => tcs.TrySetCanceled());
        purchases.LogIn(appUserId, (customerInfo, created, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new PurchasesErrorException(error, false));
            }
            else
            {
                tcs.TrySetResult(new LoginResult(customerInfo, created));
            }
        });
        return tcs.Task;
    }

    internal static Task<RCCustomerInfo> LogOutAsync(this RCPurchases purchases,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<RCCustomerInfo>();
        cancellationToken.Register(() => tcs.TrySetCanceled());
        purchases.LogOutWithCompletion((customerInfo, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new PurchasesErrorException(error, false));
            }
            else
            {
                tcs.TrySetResult(customerInfo);
            }
        });
        return tcs.Task;
    }

    internal static Task<NSDictionary<NSString, RCIntroEligibility>> CheckTrialOrIntroDiscountEligibilityAsync(this RCPurchases purchases, List<string> identifiers,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<NSDictionary<NSString, RCIntroEligibility>>();
        cancellationToken.Register(() => tcs.TrySetCanceled());
        purchases.CheckTrialOrIntroDiscountEligibility(identifiers.ToArray(), (NSDictionary<NSString, RCIntroEligibility> eligibilities) =>
        {
            tcs.TrySetResult(eligibilities);
        });
        return tcs.Task;
    }

    internal static Task<RCOfferings> GetOfferingsAsync(this RCPurchases purchases,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<RCOfferings>();
        cancellationToken.Register(() => tcs.TrySetCanceled());
        purchases.GetOfferingsWithCompletion((RCOfferings offerings, NSError error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new PurchasesErrorException(error, false));
            }
            else
            {
                tcs.TrySetResult(offerings);
            }
        });
        return tcs.Task;
    }

    internal static Task<PurchaseSuccessInfo> PurchasePackageAsync(this RCPurchases purchases,
        RCPackage packageToPurchase, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<PurchaseSuccessInfo>();
        cancellationToken.Register(() => tcs.TrySetCanceled());
        purchases.PurchasePackage(packageToPurchase,
            (RCStoreTransaction transaction, RCCustomerInfo customerInfo, NSError error, bool userCancelled) =>
            {
                if (error != null || userCancelled)
                {
                    tcs.TrySetException(new PurchasesErrorException(error, userCancelled));
                }
                else
                {
                    tcs.TrySetResult(new PurchaseSuccessInfo(transaction, customerInfo));
                }
            });
        return tcs.Task;
    }

    internal static Task<RCCustomerInfo> RestorePurchasesAsync(this RCPurchases purchases,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<RCCustomerInfo>();
        cancellationToken.Register(() => tcs.TrySetCanceled());
        purchases.RestorePurchasesWithCompletion((RCCustomerInfo customerInfo, NSError error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new PurchasesErrorException(error, false));
            }
            else
            {
                tcs.TrySetResult(customerInfo);
            }
        });
        return tcs.Task;
    }

    internal static Task<RCCustomerInfo> GetCustomerInfoAsync(this RCPurchases purchases,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<RCCustomerInfo>();
        cancellationToken.Register(() => tcs.TrySetCanceled());
        purchases.GetCustomerInfoWithCompletion((RCCustomerInfo customerInfo, NSError error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new PurchasesErrorException(error, false));
            }
            else
            {
                tcs.TrySetResult(customerInfo);
            }
        });
        return tcs.Task;
    }
}
