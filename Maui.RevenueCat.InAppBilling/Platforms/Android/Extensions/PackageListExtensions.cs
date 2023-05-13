using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class PackageListExtensions
{
    public static List<OfferingDto> ToOfferDtoList(this List<Package> packages)
    {
        var offers = new List<OfferingDto>();

        foreach (var package in packages)
        {
            var priceWithoutCurrency = package.Product.Price.Replace(package.Product.PriceCurrencyCode, string.Empty).Trim();
            var price = Convert.ToDecimal(priceWithoutCurrency);

            var offeringDto = new OfferingDto()
            {
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Price = price,
                    PriceCurrencyCode = package.Product.PriceCurrencyCode,
                    PriceWithCurrency = package.Product.Price,
                    OriginalPriceAmountMicros = package.Product.OriginalPriceAmountMicros,
                    Sku = package.Product.Sku,
                    SubscriptionPeriod = package.Product.SubscriptionPeriod,
                }
            };

            offers.Add(offeringDto);
        }

        return offers;
    }
}
