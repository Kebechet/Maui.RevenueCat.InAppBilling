using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class RCOfferingsExtensions
{
    internal static List<OfferingDto> ToOfferingDtoList(this RCOfferings offerings, IReadOnlySet<string> freeTrialEligibleProductIdentifiers)
    {
        var offeringDtos = new List<OfferingDto>();

        foreach (var offer in offerings.All.Values)
        {
            var offeringDto = new OfferingDto()
            {
                Identifier = offer.Identifier,
                AvailablePackages = offer.AvailablePackages.ToPackageDtoList(freeTrialEligibleProductIdentifiers),
                IsCurrent = offer.Identifier == offerings.Current?.Identifier,
                Metadata = offer.Metadata.ToJson().ToMetadataDictionary()
            };
            offeringDtos.Add(offeringDto);
        }

        return offeringDtos;
    }
}
