using Maui.RevenueCat.InAppBilling.Enums;
using PeriodTypeNative = Com.Revenuecat.Purchases.PeriodType;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class PeriodTypeExtensions
{
    internal static PeriodType ToDtoPeriodType(this PeriodTypeNative periodType)
    {
        if (periodType == PeriodTypeNative.Intro) return PeriodType.Intro;
        if (periodType == PeriodTypeNative.Trial) return PeriodType.Trial;
        if (periodType == PeriodTypeNative.Normal) return PeriodType.Normal;
        if (periodType == PeriodTypeNative.Prepaid) return PeriodType.Prepaid;
        throw new ArgumentException();
    }
}
