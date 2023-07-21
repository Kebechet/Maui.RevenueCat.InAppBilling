using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class PackageListExtensions
{
    public static List<OfferingDto> ToOfferDtoList(this List<Package> packages)
    {
        var offers = new List<OfferingDto>();

        foreach (var package in packages)
        {
            var currencyCode = package.Product.PriceCurrencyCode;
            var price = Convert.ToDecimal(package.Product.PriceAmountMicros * Math.Pow(10, -6));

            var offeringDto = new OfferingDto()
            {
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Pricing = new PricingDto
                    {
                        CurrencyCode = currencyCode,
                        Price = price,
                        PriceMicros = package.Product.PriceAmountMicros,
                        PriceLocalized = OfferingDtoExtensions.GetLocalizedPrice(currencyCode, price)
                    },
                    Sku = package.Product.Sku,
                    SubscriptionPeriod = package.Product.SubscriptionPeriod,
                }
            };

            offers.Add(offeringDto);
        }

        return offers;
    }
}
