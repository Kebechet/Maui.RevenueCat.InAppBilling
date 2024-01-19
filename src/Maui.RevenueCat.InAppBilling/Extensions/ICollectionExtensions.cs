namespace Maui.RevenueCat.InAppBilling.Extensions;

internal static class ICollectionExtensions
{
    internal static bool IsNullOrEmpty<T>(this ICollection<T> collection)
    {
        return collection is null || collection.Count == 0;
    }
}
