using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
internal static class OwnershipTypeExtensions
{
    internal static OwnershipType ToOwnershipType(this RCPurchaseOwnershipType ownershipType)
    {
        switch (ownershipType)
        {
            case RCPurchaseOwnershipType.FamilyShared:
                return OwnershipType.FamilyShared;
            case RCPurchaseOwnershipType.Purchased:
                return OwnershipType.Purchased;
            case RCPurchaseOwnershipType.Assigned:
                return OwnershipType.Assigned;
            default:
                return OwnershipType.Unknown;
        }
    }
}
