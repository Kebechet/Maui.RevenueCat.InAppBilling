using Maui.RevenueCat.InAppBilling.Enums;
using OwnershipTypeNative = Com.Revenuecat.Purchases.OwnershipType;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class OwnershipTypeExtensions
{
    internal static OwnershipType ToDtoOwnershipType(this OwnershipTypeNative ownershipType)
    {
        if (ownershipType == OwnershipTypeNative.FamilyShared) return OwnershipType.FamilyShared;
        if (ownershipType == OwnershipTypeNative.Purchased) return OwnershipType.Purchased;
        if (ownershipType == OwnershipTypeNative.Assigned) return OwnershipType.Assigned;
        return OwnershipType.Unknown;
    }
}
