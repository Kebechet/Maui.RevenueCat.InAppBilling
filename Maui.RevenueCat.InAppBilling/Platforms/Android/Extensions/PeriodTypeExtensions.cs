using Maui.RevenueCat.InAppBilling.Enums;
using Com.Revenuecat.Purchases;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class PeriodTypeExtensions
{
    internal static PeriodId ToPeriodId(this PeriodType period)
    {
        if (period == PeriodType.Intro) return PeriodId.Intro;
        if (period == PeriodType.Trial) return PeriodId.Trial;
        if (period == PeriodType.Normal) return PeriodId.Normal;
        throw new ArgumentException();
    }
}
