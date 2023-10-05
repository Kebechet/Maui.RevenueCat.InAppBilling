using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class RCOfferingsExtensions
{
    public static List<OfferingDto> ToOfferingDtoList(this RCOfferings offerings)
    {
        var offeringDtos = new List<OfferingDto>();

        foreach (var offer in offerings.All.Values)
        {
            var offeringDto = new OfferingDto()
            {
                Identifier = offer.Identifier,
                AvailablePackages = offer.AvailablePackages.ToPackageDtoList(),
                IsCurrent = offer.Identifier == offerings.Current?.Identifier
            };

            offeringDtos.Add(offeringDto);
        }

        return offeringDtos;
    }
}