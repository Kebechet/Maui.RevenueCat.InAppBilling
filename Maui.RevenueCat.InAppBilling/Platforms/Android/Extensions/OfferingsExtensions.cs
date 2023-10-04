using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class OfferingsExtensions
{
    internal static IList<OfferingDto> ToOfferingDtoList(this Offerings offerings)
    {
        var offeringDtos = new List<OfferingDto>();

        foreach (var offer in offerings.All.Values)
        {
            var offerDto = new OfferingDto()
            {
                Identifier = offer.Identifier,
                AvailablePackages = offer.AvailablePackages.ToPackageDtoList().ToList(),
                IsCurrent = offer.Identifier == offerings.Current.Identifier
            };

            offeringDtos.Add(offerDto);
        }

        return offeringDtos;
    }
}
