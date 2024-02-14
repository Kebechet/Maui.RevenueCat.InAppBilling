using System.Collections;
using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Extensions;
using Maui.RevenueCat.InAppBilling.Models;
using Org.Json;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class OfferingsExtensions
{
    internal static List<OfferingDto> ToOfferingDtoList(this Offerings offerings)
    {
        var offeringDtos = new List<OfferingDto>();

        foreach (var offer in offerings.All.Values)
        {
            var offerDto = new OfferingDto()
            {
                Identifier = offer.Identifier,
                AvailablePackages = offer.AvailablePackages.ToPackageDtoList(),
                IsCurrent = offer.Identifier == offerings?.Current?.Identifier,
                Metadata = offer.Metadata.IsNullOrEmpty()
                    ? null
                    : new JSONObject((IDictionary)offer.Metadata).ToString()
            };

            offeringDtos.Add(offerDto);
        }

        return offeringDtos;
    }
}