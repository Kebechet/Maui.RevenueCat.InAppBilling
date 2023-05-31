using Maui.RevenueCat.InAppBilling.Models;
using System.Text.RegularExpressions;

namespace Maui.RevenueCat.InAppBilling.Extensions;

public static partial class OfferingDtoExtensions
{
    private static readonly decimal _daysInWeek = 7m;
    private static readonly decimal _daysInMonth = 30m;
    private static readonly decimal _monthsInBiMonthly = 2m;
    private static readonly decimal _quartalsInYear = 4m;
    private static readonly decimal _monthsInHalfYear = 6m;
    private static readonly decimal _monthsInYear = 12m;

    [GeneratedRegex(@"(\d+)[\.\,]{1}")]
    private static partial Regex BeforeSeparatorValueRegex();
    [GeneratedRegex(@"[\.\,]{1}(\d+)")]
    private static partial Regex AfterSeparatorValueRegex();
    [GeneratedRegex(@"\d+([\.\,]{1})\d+")]
    private static partial Regex SeparatorValueRegex();

    public static decimal GetMonthlyPrice(this OfferingDto offeringDto, bool ignoreExceptions = true)
    {
        if (offeringDto.Identifier == DefaultOfferingIdentifier.Weekly)
        {
            return offeringDto.Product.Pricing.Price / _daysInWeek * _daysInMonth;
        }

        if (offeringDto.Identifier == DefaultOfferingIdentifier.Monthly)
        {
            return offeringDto.Product.Pricing.Price;
        }

        if (offeringDto.Identifier == DefaultOfferingIdentifier.BiMonthly)
        {
            return offeringDto.Product.Pricing.Price / _monthsInBiMonthly;
        }

        if (offeringDto.Identifier == DefaultOfferingIdentifier.Quarterly)
        {
            return offeringDto.Product.Pricing.Price / _quartalsInYear;
        }

        if (offeringDto.Identifier == DefaultOfferingIdentifier.SemiAnnually)
        {
            return offeringDto.Product.Pricing.Price / _monthsInHalfYear;
        }

        if (offeringDto.Identifier == DefaultOfferingIdentifier.Annually)
        {
            return offeringDto.Product.Pricing.Price / _monthsInYear;
        }

        if (ignoreExceptions)
        {
            return 0m;
        }

        throw new NotImplementedException("Specified offering identifier is not supported.");
    }

    public static string GetMonthlyPriceWithCurrency(this OfferingDto offeringDto, bool ignoreExceptions = true)
    {
        try
        {
            //the reason for not using directly CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator
            //is to persist formatting of the price as it is in the Payment vendor
            var monthlyPrice = $"{offeringDto.GetMonthlyPrice(ignoreExceptions):n2}";
            var monthlyPriceBeforeSeparator = BeforeSeparatorValueRegex().Match(monthlyPrice).Groups[1].Value;
            var monthlyPriceAfterSeparator = AfterSeparatorValueRegex().Match(monthlyPrice).Groups[1].Value;

            return SeparatorValueRegex().Replace(offeringDto.Product.Pricing.PriceLocalized, $"{monthlyPriceBeforeSeparator}${{1}}{monthlyPriceAfterSeparator}");
        }
        catch
        {
            if (ignoreExceptions)
            {
                return "$0.00";
            }

            throw;
        }
    }
}
