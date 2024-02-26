using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class CustomerInfoExtensions
{
    internal static CustomerInfoDto ToCustomerInfoDto(this CustomerInfo customerInfo)
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
}
