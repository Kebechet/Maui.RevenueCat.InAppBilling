using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
internal static class RCEntitlementInfosExtensions
{
    internal static List<EntitlementInfoDto> ToEntitlementInfoDtoList(this RCEntitlementInfos entitlements)
    {
        var entitlementInfoDtos = new List<EntitlementInfoDto>();

        foreach (var entitlement in entitlements.All.Values)
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
                OwnershipType = entitlement.OwnershipType.ToOwnershipId(),
                PeriodType = entitlement.PeriodType.ToPeriodId(),
                ProductIdentifier = entitlement.ProductIdentifier,
                ProductPlanIdentifier = string.Empty, // Does not exist on ios???
                Store = entitlement.Store.ToStoreId(),
                UnsubscribeDetectedAt = entitlement.UnsubscribeDetectedAt.ToDateTime(),
                WillRenew = entitlement.WillRenew,
            };

            entitlementInfoDtos.Add(entitlementInfoDto);
        }

        return entitlementInfoDtos;
    }
}
