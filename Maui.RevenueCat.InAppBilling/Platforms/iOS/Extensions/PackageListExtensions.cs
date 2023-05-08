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
            var offeringDto = new OfferingDto()
            {
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Price = package.StoreProduct.LocalizedPriceString,
                    PriceCurrencyCode = package.StoreProduct.CurrencyCode ?? string.Empty,
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