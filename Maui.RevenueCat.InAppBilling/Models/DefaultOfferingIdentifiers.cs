namespace Maui.RevenueCat.InAppBilling.Models;

//https://github.com/RevenueCat/purchases-ios/blob/main/Sources/Purchasing/PackageType.swift#L66
public static class DefaultOfferingIdentifier
{
    public static string Weekly => "$rc_weekly";
    public static string Monthly => "$rc_monthly";
    public static string BiMonthly => "$rc_two_month";
    public static string Quarterly => "$rc_three_month";
    public static string SemiAnnually => "$rc_six_month";
    public static string Annually => "$rc_annual";
    public static string Lifetime => "$rc_lifetime";
}
