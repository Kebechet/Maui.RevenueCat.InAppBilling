namespace Maui.RevenueCat.InAppBilling.Extensions;

internal static class StringExtensions
{
    internal static bool IsNullOrEmpty(this string value)
    {
        return value is null || value == string.Empty;
    }
}
