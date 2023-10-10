using Maui.RevenueCat.InAppBilling.Enums;
using Com.Revenuecat.Purchases;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class PeriodTypeExtensions
{
    internal static PeriodId ToPeriodId(this PeriodType periodType)
    {
        if (periodType == PeriodType.Intro) return PeriodId.Intro;
        if (periodType == PeriodType.Trial) return PeriodId.Trial;
        if (periodType == PeriodType.Normal) return PeriodId.Normal;
        throw new ArgumentException();
    }
}
