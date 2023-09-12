#if IOS
using Maui.RevenueCat.iOS;
#endif
namespace Maui.RevenueCat.InAppBilling.Enums;

public enum IntroElegibilityStatus
{
    Eligible,
    Ineligible,
    NoIntroOfferExists,
    Unknown
}

#if IOS
public static class Extensions
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
#endif