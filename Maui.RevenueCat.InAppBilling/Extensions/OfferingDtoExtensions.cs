﻿using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Extensions;

public static class OfferingDtoExtensions
{
    private static readonly decimal _daysInWeek = 7m;
    private static readonly decimal _daysInMonth = 30m;
    private static readonly decimal _monthsInBiMonthly = 2m;
    private static readonly decimal _quartalsInYear = 4m;
    private static readonly decimal _monthsInHalfYear = 6m;
    private static readonly decimal _monthsInYear = 12m;

    public static decimal GetMonthlyPrice(this OfferingDto offeringDto)
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

        throw new NotImplementedException();
    }

    public static string GetMonthlyPriceWithCurrency(this OfferingDto offeringDto)
    {
        return offeringDto.Product.Pricing.PriceLocalized
            .Replace($"{offeringDto.Product.Pricing.Price:n2}", $"{offeringDto.GetMonthlyPrice():n2}");
    }
}