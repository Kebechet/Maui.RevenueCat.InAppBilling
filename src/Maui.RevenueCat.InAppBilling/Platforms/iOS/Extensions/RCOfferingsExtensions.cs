using Foundation;
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
            string? jsonString = null;

            if (!offer.Metadata.IsNullOrEmpty())
            {
                var jsonData =
                    NSJsonSerialization.Serialize(offer.Metadata, NSJsonWritingOptions.PrettyPrinted, out var error);
                jsonString = error is null
                    ? NSString.FromData(jsonData, NSStringEncoding.UTF8)
                    : null;
            }

            var offeringDto = new OfferingDto()
            {
                Identifier = offer.Identifier,
                AvailablePackages = offer.AvailablePackages.ToPackageDtoList(),
                IsCurrent = offer.Identifier == offerings.Current?.Identifier,
                Metadata = jsonString
            };
            offeringDtos.Add(offeringDto);
        }

        return offeringDtos;
    }
}