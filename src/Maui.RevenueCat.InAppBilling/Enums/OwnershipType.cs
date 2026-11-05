namespace Maui.RevenueCat.InAppBilling.Enums;

public enum OwnershipType
{
    FamilyShared,
    Purchased,
    Unknown,

    /// <summary>
    /// The user has access to the purchase through an organization (StoreKit's <c>assigned</c> ownership).
    /// </summary>
    Assigned,
}
