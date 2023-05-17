using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Extensions;

public static class OfferingDtoExtensions
{
    private static readonly decimal _daysInWeek = 7m;
    private static readonly decimal _daysInMonth = 30m;
    private static readonly decimal _monthsInBiMonthly = 2m;
    private static readonly decimal _quartalsInYear = 4m;
    private static readonly decimal _monthsInHalfYear = 6m;
    private static readonly decimal _monthsInYear = 12m;

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
            return offeringDto.Product.Pricing.PriceLocalized
            .Replace($"{offeringDto.Product.Pricing.Price:n2}", $"{offeringDto.GetMonthlyPrice(ignoreExceptions):n2}");
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
