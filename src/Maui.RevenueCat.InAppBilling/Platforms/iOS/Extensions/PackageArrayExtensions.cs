using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class PackageArrayExtensions
{
    /// <param name="freeTrialEligibleProductIdentifiers">
    /// Products whose free trial the user can still start; every other product is mapped without one.
    /// </param>
    internal static List<PackageDto> ToPackageDtoList(this RCPackage[] packages, IReadOnlySet<string> freeTrialEligibleProductIdentifiers)
    {
        var packageDtos = new List<PackageDto>();

        foreach (var package in packages)
        {
            var currencyCode = package.StoreProduct.CurrencyCode ?? string.Empty;
            var price = Convert.ToDecimal(package.StoreProduct.Price.DoubleValue);

            var packageDto = new PackageDto()
            {
                OfferingIdentifier = package.OfferingIdentifier,
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Pricing = new PricingDto
                    {
                        CurrencyCode = currencyCode,
                        Price = price,
                        PriceMicros = (long)(package.StoreProduct.Price.DoubleValue * Math.Pow(10, 6)),
                        PriceLocalized = PackageDtoExtensions.GetLocalizedPrice(currencyCode, price)
                    },
                    Sku = package.StoreProduct.ProductIdentifier,
                    SubscriptionPeriod = package.StoreProduct.SubscriptionPeriod.ToSubscriptionPeriodDto(),
                    FreeTrialPeriod = freeTrialEligibleProductIdentifiers.Contains(package.StoreProduct.ProductIdentifier)
                        ? package.StoreProduct.IntroductoryDiscount.ToFreeTrialPeriodDto()
                        : null,
                }
            };

            packageDtos.Add(packageDto);
        }

        return packageDtos;
    }
}