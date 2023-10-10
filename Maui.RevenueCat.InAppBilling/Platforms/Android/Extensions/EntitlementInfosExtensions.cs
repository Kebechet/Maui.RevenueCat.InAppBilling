using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class EntitlementInfosExtensions
{
    internal static List<EntitlementInfoDto> ToEntitlementInfoDtoList(this EntitlementInfos entitlementInfos)
    {
        var entitlementInfoDtos = new List<EntitlementInfoDto>();

        foreach (var entitlement in entitlementInfos.All.Values)
        {
            var entitlementInfoDto = new EntitlementInfoDto()
            {
                BillingIssueDetectedAt = entitlement.BillingIssueDetectedAt.ToDateTime(),
                ExpirationDate = entitlement.ExpirationDate.ToDateTime(),
                Identifier = entitlement.Identifier,
                IsActive = entitlement.IsActive,
                IsSandbox = entitlement.IsSandbox,
                LatestPurchaseDate = entitlement.LatestPurchaseDate.ToDateTime(),
                OriginalPurchaseDate = entitlement.OriginalPurchaseDate.ToDateTime(),
                OwnershipType = entitlement.OwnershipType.ToOwnershipTypeFromNative(),
                PeriodType = entitlement.PeriodType.ToPeriodTypeFromNative(),
                ProductIdentifier = entitlement.ProductIdentifier,
                ProductPlanIdentifier = entitlement.ProductPlanIdentifier,
                Store = entitlement.Store.ToStoreId(),
                UnsubscribeDetectedAt = entitlement.UnsubscribeDetectedAt.ToDateTime(),
                WillRenew = entitlement.WillRenew,
            };

            entitlementInfoDtos.Add(entitlementInfoDto);
        }

        return entitlementInfoDtos;
    }
}
