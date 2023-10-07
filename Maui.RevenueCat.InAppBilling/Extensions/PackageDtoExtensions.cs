using Maui.RevenueCat.InAppBilling.Models;
using System.Diagnostics;
using System.Globalization;

namespace Maui.RevenueCat.InAppBilling.Extensions;

public static partial class PackageDtoExtensions
{
    private static readonly decimal _daysInWeek = 7m;
    private static readonly decimal _daysInMonth = 30m;
    private static readonly decimal _monthsInBiMonthly = 2m;
    private static readonly decimal _quartalsInYear = 4m;
    private static readonly decimal _monthsInHalfYear = 6m;
    private static readonly decimal _monthsInYear = 12m;

    public static decimal GetMonthlyPrice(this PackageDto packageDto, bool ignoreExceptions = true)
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
                result = packageDto.Product.Pricing.Price / _quartalsInYear;
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

        return result.RoundUp(2);
    }

    public static string GetMonthlyPriceWithCurrency(this PackageDto packageDto, bool ignoreExceptions = true)
    {
        try
        {
            var monthlyPrice = packageDto.GetMonthlyPrice(ignoreExceptions);

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

    internal static string GetLocalizedPrice(string priceIsoCurrencyCode, decimal price)
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
