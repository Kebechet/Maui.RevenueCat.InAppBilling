using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Extensions;
internal static class IntroElegibilityStatusExtensions
{
    public static IntroElegibilityStatus Convert(this RCIntroEligibilityStatus eligibility)
    {
        switch (eligibility)
        {
            case RCIntroEligibilityStatus.Ineligible:
                return IntroElegibilityStatus.Ineligible;
            case RCIntroEligibilityStatus.Eligible:
                return IntroElegibilityStatus.Eligible;
            case RCIntroEligibilityStatus.NoIntroOfferExists:
                return IntroElegibilityStatus.NoIntroOfferExists;
            case RCIntroEligibilityStatus.Unknown:
            default:
                return IntroElegibilityStatus.Unknown;
        }
    }
}
