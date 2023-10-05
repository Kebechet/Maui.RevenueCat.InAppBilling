using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
internal static class OwnershipTypeExtensions
{
    internal static OwnershipId ToOwnershipId(this RCPurchaseOwnershipType owner)
    {
        switch (owner)
        {
            case RCPurchaseOwnershipType.FamilyShared:
                return OwnershipId.FamilyShared;
            case RCPurchaseOwnershipType.Purchased:
                return OwnershipId.Purchased;
            default:
                return OwnershipId.Unknown;
        }
    }
}
