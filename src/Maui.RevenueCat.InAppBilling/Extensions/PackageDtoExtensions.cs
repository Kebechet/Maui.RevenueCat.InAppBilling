using Maui.RevenueCat.InAppBilling.Models;
using System.Diagnostics;
using System.Globalization;

namespace Maui.RevenueCat.InAppBilling.Extensions;

public static partial class PackageDtoExtensions
{
    private static readonly decimal _daysInWeek = 7m;
    private static readonly decimal _daysInMonth = 30m;
    private static readonly decimal _monthsInBiMonthly = 2m;
    private static readonly decimal _monthsInQuartal = 3m;
    private static readonly decimal _monthsInHalfYear = 6m;
    private static readonly decimal _monthsInYear = 12m;

    public static decimal GetMonthlyPrice(this PackageDto packageDto, bool ignoreExceptions = true, int? decimalRoundUpTo = 2)
    {
        decimal result;

        switch (packageDto.Identifier)
        {
            case DefaultPackageIdentifier.Weekly:
                result = packageDto.Product.Pricing.Price / _daysInWeek * _daysInMonth;
                break;
            case DefaultPackageIdentifier.Monthly:
                result = packageDto.Product.Pricing.Price;
                break;
            case DefaultPackageIdentifier.BiMonthly:
                result = packageDto.Product.Pricing.Price / _monthsInBiMonthly;
                break;
            case DefaultPackageIdentifier.Quarterly:
                result = packageDto.Product.Pricing.Price / _monthsInQuartal;
                break;
            case DefaultPackageIdentifier.SemiAnnually:
                result = packageDto.Product.Pricing.Price / _monthsInHalfYear;
                break;
            case DefaultPackageIdentifier.Annually:
                result = packageDto.Product.Pricing.Price / _monthsInYear;
                break;
            default:
                if (ignoreExceptions)
                {
                    result = 0m;
                    break;
                }
                throw new NotImplementedException("Specified offering identifier is not supported.");
        }

        return decimalRoundUpTo is null
            ? result
            : result.RoundUp(decimalRoundUpTo.Value);
    }
    public static decimal GetWeeklyPrice(this PackageDto packageDto, bool ignoreExceptions = true, int? decimalRoundUpTo = 2)
    {
        var monthlyPrice = GetMonthlyPrice(packageDto, ignoreExceptions, null);
        var weeklyPrice = monthlyPrice / _daysInMonth * _daysInWeek;

        return decimalRoundUpTo is null
            ? weeklyPrice
            : weeklyPrice.RoundUp(decimalRoundUpTo.Value);
    }

    public static string GetMonthlyPriceWithCurrency(this PackageDto packageDto, bool ignoreExceptions = true, int? decimalRoundUpTo = 2)
    {
        try
        {
            var monthlyPrice = packageDto.GetMonthlyPrice(ignoreExceptions, decimalRoundUpTo);

            var localisedCurrency = GetLocalizedPrice(packageDto.Product.Pricing.CurrencyCode, monthlyPrice);

            return localisedCurrency;
        }
        catch (Exception)
        {
            if (ignoreExceptions)
            {
                return "$0.00";
            }

            throw;
        }
    }
    public static string GetWeeklyPriceWithCurrency(this PackageDto packageDto, bool ignoreExceptions = true, int? decimalRoundUpTo = 2)
    {
        try
        {
            var weeklyPrice = packageDto.GetWeeklyPrice(ignoreExceptions, decimalRoundUpTo);

            var localisedCurrency = GetLocalizedPrice(packageDto.Product.Pricing.CurrencyCode, weeklyPrice);

            return localisedCurrency;
        }
        catch (Exception)
        {
            if (ignoreExceptions)
            {
                return "$0.00";
            }

            throw;
        }
    }

    public static string GetLocalizedPrice(string priceIsoCurrencyCode, decimal price)
    {
        var currencyCulture = GetCulture(priceIsoCurrencyCode);

        return price.ToString("C", currencyCulture);
    }

    private static CultureInfo GetCulture(string isoCurrencySymbol)
    {
        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            var regionalInfo = new RegionInfo(ci.Name);
            if (regionalInfo.ISOCurrencySymbol == isoCurrencySymbol)
            {
                return ci;
            }
        }

        Debug.WriteLine("Culture not found for " + isoCurrencySymbol);

        return CultureInfo.CurrentCulture;
    }
}
