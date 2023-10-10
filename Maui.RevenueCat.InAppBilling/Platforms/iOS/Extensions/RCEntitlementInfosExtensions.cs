using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
internal static class RCEntitlementInfosExtensions
{
    internal static List<EntitlementInfoDto> ToEntitlementInfoDtoList(this RCEntitlementInfos entitlementInfos)
    {
        var entitlementInfoDtos = new List<EntitlementInfoDto>();

        foreach (var entitlement in entitlementInfos.All.Values)
        {
            var entitlementInfoDto = new EntitlementInfoDto()
            {
                BillingIssueDetectedAt = entitlement.BillingIssueDetectedAt?.ToDateTime(),
                ExpirationDate = entitlement.ExpirationDate.ToDateTime(),
                Identifier = entitlement.Identifier,
                IsActive = entitlement.IsActive,
                IsSandbox = entitlement.IsSandbox,
                LatestPurchaseDate = entitlement.LatestPurchaseDate.ToDateTime(),
                OriginalPurchaseDate = entitlement.OriginalPurchaseDate.ToDateTime(),
                OwnershipType = entitlement.OwnershipType.ToOwnershipType(),
                PeriodType = entitlement.PeriodType.ToPeriodType(),
                ProductIdentifier = entitlement.ProductIdentifier,
                ProductPlanIdentifier = string.Empty, // this does not exist on iOS
                Store = entitlement.Store.ToStoreId(),
                UnsubscribeDetectedAt = entitlement.UnsubscribeDetectedAt.ToDateTime(),
                WillRenew = entitlement.WillRenew,
            };

            entitlementInfoDtos.Add(entitlementInfoDto);
        }

        return entitlementInfoDtos;
    }
}
