using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class OfferingListExtensions
{
    public static IList<OfferingDto> ToOfferingDtoList(this Offerings offerings)
    {
        var offersDto = new List<OfferingDto>();

        foreach (var offer in offerings.All.Values)
        {
            var offerDto = new OfferingDto()
            {
                Identifier = offer.Identifier,
                AvailablePackages = offer.AvailablePackages.ToList().ToPackageDtoList(),
                IsCurrent = offer.Identifier == offerings.Current.Identifier
            };

            offersDto.Add(offerDto);
        }

        return offersDto;
    }
}
