using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;
internal static class PeriodTypeExtensions
{
    internal static PeriodId ToPeriodId(this RCPeriodType periodType)
    {
        switch (periodType)
        {
            case RCPeriodType.Intro:
                return PeriodId.Intro;
            case RCPeriodType.Trial:
                return PeriodId.Trial;
            case RCPeriodType.Normal:
                return PeriodId.Normal;
            default:
                throw new ArgumentException();
        }
    }
}
