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
            var priceWithoutCurrency = package.StoreProduct.LocalizedPriceString.Replace(package.StoreProduct.CurrencyCode!, string.Empty).Trim();
            var price = Convert.ToDecimal(priceWithoutCurrency);

            var offeringDto = new OfferingDto()
            {
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Price = price,
                    PriceCurrencyCode = package.StoreProduct.CurrencyCode ?? string.Empty,
                    PriceWithCurrency = package.StoreProduct.LocalizedPriceString,
                    OriginalPriceAmountMicros = (long)package.StoreProduct.Price,
                    Sku = package.StoreProduct.ProductIdentifier,
                    SubscriptionPeriod = package.StoreProduct.SubscriptionPeriod.ToString(),
                }
            };

            offers.Add(offeringDto);
        }

        return offers;
    }
}