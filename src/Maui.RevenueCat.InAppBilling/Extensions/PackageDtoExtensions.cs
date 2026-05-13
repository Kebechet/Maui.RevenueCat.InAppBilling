using Maui.RevenueCat.InAppBilling.Models;
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

    /// <summary>
    /// Formats <paramref name="price"/> as a localized currency string with deterministic output:
    /// the same input always produces the same string on any device. Number conventions
    /// (separators, grouping) come from <see cref="CultureInfo.CurrentCulture"/>; the currency
    /// symbol and decimal-digit count come from the first specific culture whose
    /// <see cref="RegionInfo.ISOCurrencySymbol"/> matches <paramref name="priceIsoCurrencyCode"/>,
    /// picked by ordinal name ordering. Falls back to using the ISO code as the symbol if no
    /// culture matches. Whole-number prices drop the fractional part (<c>199 Kč</c>, not
    /// <c>199,00 Kč</c>).
    /// </summary>
    public static string GetLocalizedPrice(string priceIsoCurrencyCode, decimal price)
    {
        var format = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();

        // Ordinal sort = deterministic across runs/devices. The same ISO code always picks
        // the same culture, so the symbol and decimal-digit count never drift.
        var currencyCulture = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .OrderBy(static c => c.Name, StringComparer.Ordinal)
            .FirstOrDefault(c =>
            {
                try { return new RegionInfo(c.Name).ISOCurrencySymbol == priceIsoCurrencyCode; }
                catch { return false; }
            });

        if (currencyCulture is not null)
        {
            format.CurrencyDecimalDigits = currencyCulture.NumberFormat.CurrencyDecimalDigits;
            format.CurrencySymbol = currencyCulture.NumberFormat.CurrencySymbol;
        }
        else
        {
            // No matched culture — keep the ISO code as the symbol so the output is at
            // least readable. Decimal digits stay at CurrentCulture's default.
            format.CurrencySymbol = priceIsoCurrencyCode;
        }

        return price == Math.Floor(price)
            ? price.ToString("C0", format)
            : price.ToString("C", format);
    }
}
