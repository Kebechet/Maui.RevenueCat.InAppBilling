namespace Maui.RevenueCat.InAppBilling.Extensions;

internal static class DecimalExtensions
{
    internal static decimal RoundUp(this decimal number, int decimals)
    {
        var multiplier = (decimal)Math.Pow(10, decimals);
        return Math.Ceiling(number * multiplier) / multiplier;
    }
}
