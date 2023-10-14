using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
internal static class PeriodTypeExtensions
{
    internal static PeriodType ToPeriodType(this RCPeriodType periodType)
    {
        switch (periodType)
        {
            case RCPeriodType.Intro:
                return PeriodType.Intro;
            case RCPeriodType.Trial:
                return PeriodType.Trial;
            case RCPeriodType.Normal:
                return PeriodType.Normal;
            default:
                throw new ArgumentException();
        }
    }
}
