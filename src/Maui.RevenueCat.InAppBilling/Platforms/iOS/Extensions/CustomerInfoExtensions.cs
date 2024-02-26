using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class CustomerInfoExtensions
{
    internal static CustomerInfoDto ToCustomerInfoDto(this RCCustomerInfo customerInfo)
    {
        return new CustomerInfoDto()
        {
            ActiveSubscriptions = customerInfo.ActiveSubscriptions.ToStringList(),
            AllPurchasedIdentifiers = customerInfo.AllPurchasedProductIdentifiers.ToStringList(),
            NonConsumablePurchases = customerInfo.NonConsumablePurchases.ToStringList(),
            FirstSeen = customerInfo.FirstSeen.ToDateTime(),
            LatestExpirationDate = customerInfo.LatestExpirationDate.ToDateTime(),
            ManagementURL = customerInfo.ManagementURL?.ToString(),
            Entitlements = customerInfo.Entitlements.ToEntitlementInfoDtoList(),
        };
    }
}
