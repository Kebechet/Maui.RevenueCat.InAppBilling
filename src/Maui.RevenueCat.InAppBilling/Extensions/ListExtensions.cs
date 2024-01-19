namespace Maui.RevenueCat.InAppBilling.Extensions;

internal static class ListExtensions
{
    internal static bool IsNullOrEmpty<T>(this List<T> list)
    {
        return list is null || list.Count == 0;
    }
}
