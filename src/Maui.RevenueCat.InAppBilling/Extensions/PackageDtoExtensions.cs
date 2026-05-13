using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using System.Globalization;

namespace Maui.RevenueCat.InAppBilling.Extensions;

/// <summary>
/// Display helpers for <see cref="PackageDto"/>: converts a package's base price into
/// any per-period figure (<see cref="GetPriceFor"/>), formats it as a deterministic
/// localized currency string (<see cref="GetPriceWithCurrencyFor"/>,
/// <see cref="GetLocalizedPrice"/>), and rounds via <see cref="DecimalExtensions.RoundUp"/>.
/// </summary>
public static partial class PackageDtoExtensions
{
    private static readonly decimal _daysInWeek = 7m;
    private static readonly decimal _daysInMonth = 30m;
    private static readonly decimal _monthsInBiMonthly = 2m;
    private static readonly decimal _monthsInQuartal = 3m;
    private static readonly decimal _monthsInHalfYear = 6m;
    private static readonly decimal _monthsInYear = 12m;

    /// <summary>
    /// Returns the package's price normalized to <paramref name="duration"/>. For a yearly
    /// subscription with <paramref name="duration"/> = <see cref="PriceDuration.Monthly"/>
    /// this returns one-twelfth of the package price. For a weekly subscription with
    /// <paramref name="duration"/> = <see cref="PriceDuration.Yearly"/> it returns the
    /// weekly price times the number of weeks in a year. Pass
    /// <paramref name="decimalRoundUpTo"/> = <c>null</c> to skip rounding.
    /// </summary>
    public static decimal GetPriceFor(
        this PackageDto packageDto,
        PriceDuration duration,
        bool ignoreExceptions = true,
        int? decimalRoundUpTo = 2)
    {
        var monthlyPrice = NormalizeToMonthly(packageDto, ignoreExceptions);

        var result = duration switch
        {
            PriceDuration.Daily => monthlyPrice / _daysInMonth,
            PriceDuration.Weekly => monthlyPrice / _daysInMonth * _daysInWeek,
            PriceDuration.Monthly => monthlyPrice,
            PriceDuration.Yearly => monthlyPrice * _monthsInYear,
            _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, "Unknown PriceDuration."),
        };

        return decimalRoundUpTo is null
            ? result
            : result.RoundUp(decimalRoundUpTo.Value);
    }

    /// <summary>
    /// Like <see cref="GetPriceFor"/> but returns a localized currency string (e.g. <c>199 Kč</c>)
    /// via <see cref="GetLocalizedPrice"/>. Swallows any exception when
    /// <paramref name="ignoreExceptions"/> is <c>true</c> and returns <c>"$0.00"</c>.
    /// </summary>
    public static string GetPriceWithCurrencyFor(
        this PackageDto packageDto,
        PriceDuration duration,
        bool ignoreExceptions = true,
        int? decimalRoundUpTo = 2)
    {
        try
        {
            var price = packageDto.GetPriceFor(duration, ignoreExceptions, decimalRoundUpTo);
            return GetLocalizedPrice(packageDto.Product.Pricing.CurrencyCode, price);
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

    // Maps each DefaultPackageIdentifier to its equivalent monthly figure. Single source
    // of truth for all per-period conversions; GetPriceFor then scales monthly -> target
    // duration with a simple ratio.
    private static decimal NormalizeToMonthly(PackageDto packageDto, bool ignoreExceptions)
    {
        var price = packageDto.Product.Pricing.Price;

        return packageDto.Identifier switch
        {
            DefaultPackageIdentifier.Weekly => price / _daysInWeek * _daysInMonth,
            DefaultPackageIdentifier.Monthly => price,
            DefaultPackageIdentifier.BiMonthly => price / _monthsInBiMonthly,
            DefaultPackageIdentifier.Quarterly => price / _monthsInQuartal,
            DefaultPackageIdentifier.SemiAnnually => price / _monthsInHalfYear,
            DefaultPackageIdentifier.Annually => price / _monthsInYear,
            _ => ignoreExceptions
                ? 0m
                : throw new NotImplementedException("Specified offering identifier is not supported."),
        };
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
