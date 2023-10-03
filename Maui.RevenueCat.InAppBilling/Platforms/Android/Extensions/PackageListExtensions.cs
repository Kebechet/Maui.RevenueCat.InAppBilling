using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class PackageListExtensions
{
    public static List<PackageDto> ToPackageDtoList(this List<Package> packages)
    {
        var packageDtos = new List<PackageDto>();

        foreach (var package in packages)
        {
            var currencyCode = package.Product.Price.CurrencyCode;
            var price = Convert.ToDecimal(package.Product.Price.AmountMicros * Math.Pow(10, -6));

            var packageDto = new PackageDto()
            {
                OfferingIdentifier = package.Offering,
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Pricing = new PricingDto
                    {
                        CurrencyCode = currencyCode,
                        Price = price,
                        PriceMicros = package.Product.Price.AmountMicros,
                        PriceLocalized = PackageDtoExtensions.GetLocalizedPrice(currencyCode, price)
                    },
                    Sku = package.Product.Sku,
                    SubscriptionPeriod = package.Product.Period?.ToString() ?? string.Empty,
                }
            };

            packageDtos.Add(packageDto);
        }

        return packageDtos;
    }
}
