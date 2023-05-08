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
            var offeringDto = new OfferingDto()
            {
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Price = package.Product.Price,
                    PriceCurrencyCode = package.Product.PriceCurrencyCode,
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
