using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class PackageListExtensions
{
    internal static List<OfferingDto> ToOfferDtoList(this List<RCPackage> packages)
    {
        var offers = new List<OfferingDto>();

        foreach (var package in packages)
        {
            var currencyCode = package.StoreProduct.CurrencyCode ?? string.Empty;
            var price = Convert.ToDecimal(package.StoreProduct.Price.DoubleValue);

            var offeringDto = new OfferingDto()
            {
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Pricing = new PricingDto
                    {
                        CurrencyCode = currencyCode,
                        Price = price,
                        PriceMicros = (long)(package.StoreProduct.Price.DoubleValue * Math.Pow(10, 6)),
                        PriceLocalized = OfferingDtoExtensions.GetLocalizedPrice(currencyCode, price)
                    },
                    Sku = package.StoreProduct.ProductIdentifier,
                    SubscriptionPeriod = package.StoreProduct.SubscriptionPeriod.ToString(),
                }
            };

            offers.Add(offeringDto);
        }

        return offers;
    }
}