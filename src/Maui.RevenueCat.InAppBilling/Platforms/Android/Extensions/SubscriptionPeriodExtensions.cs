using Com.Revenuecat.Purchases.Models;
using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class SubscriptionPeriodExtensions
{
    internal static SubscriptionPeriodDto? ToSubscriptionPeriodDto(this Period? period)
    {
        if (period is null)
        {
            return null;
        }

        return new SubscriptionPeriodDto
        {
            Value = period.Value,
            Unit = UnitFromIso8601(period.Iso8601),
        };
    }

    // Reads the unit from Period.Iso8601 (always ends with D/W/M/Y) rather than
    // period.Unit — the bound C# property `Unit` collides with the nested type
    // `Period.Unit` (CS0572 / CS0119).
    private static SubscriptionUnit UnitFromIso8601(string? iso) =>
        string.IsNullOrEmpty(iso) || iso.Length < 2
            ? SubscriptionUnit.Unknown
            : iso[^1] switch
            {
                'D' => SubscriptionUnit.Day,
                'W' => SubscriptionUnit.Week,
                'M' => SubscriptionUnit.Month,
                'Y' => SubscriptionUnit.Year,
                _   => SubscriptionUnit.Unknown,
            };
}
