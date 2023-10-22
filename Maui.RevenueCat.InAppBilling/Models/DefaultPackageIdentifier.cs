namespace Maui.RevenueCat.InAppBilling.Models;

//https://github.com/RevenueCat/purchases-ios/blob/main/Sources/Purchasing/PackageType.swift#L66
public static class DefaultPackageIdentifier
{
    public const string Weekly = "$rc_weekly";
    public const string Monthly = "$rc_monthly";
    public const string BiMonthly = "$rc_two_month";
    public const string Quarterly = "$rc_three_month";
    public const string SemiAnnually = "$rc_six_month";
    public const string Annually = "$rc_annual";
    public const string Lifetime = "$rc_lifetime";
}
