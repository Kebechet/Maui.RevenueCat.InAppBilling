namespace Maui.RevenueCat.InAppBilling.Extensions;

internal static class IDictonaryExtensions
{
    internal static bool IsNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dict)
    {
        return dict is null || dict.Count == 0;
    }
}
