using Maui.RevenueCat.InAppBilling.Enums;
using Com.Revenuecat.Purchases;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class OwnershipTypeExtensions
{
    internal static OwnershipId ToOwnershipId(this OwnershipType owner)
    {
        if (owner == OwnershipType.FamilyShared) return OwnershipId.FamilyShared;
        if (owner == OwnershipType.Purchased) return OwnershipId.Purchased;
        return OwnershipId.Unknown;
    }
}
