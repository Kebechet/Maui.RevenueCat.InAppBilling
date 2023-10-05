using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class EntitlementInfosExtensions
{
    internal static IList<EntitlementInfoDto> ToEntitlementInfoDtoList(this EntitlementInfos entitlements)
    {
        var entitlementInfoDtos = new List<EntitlementInfoDto>();

        foreach (var entitlement in entitlements.All.Values)
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
                OwnershipType = entitlement.OwnershipType.ToOwnershipId(),
                PeriodType = entitlement.PeriodType.ToPeriodId(),
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
