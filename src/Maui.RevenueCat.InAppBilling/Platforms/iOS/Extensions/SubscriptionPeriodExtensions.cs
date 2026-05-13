using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class SubscriptionPeriodExtensions
{
    internal static SubscriptionPeriodDto? ToSubscriptionPeriodDto(this RCSubscriptionPeriod? period)
    {
        if (period is null)
        {
            return null;
        }

        return new SubscriptionPeriodDto
        {
            Value = (int)period.Value,
            Unit = period.Unit.ToSubscriptionUnit(),
        };
    }
}
