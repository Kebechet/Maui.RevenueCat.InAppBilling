using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Extensions;
public static partial class OfferingDtoExtensions
{
    public static OfferingDto GetCurrent(this List<OfferingDto> offerings)
    {
        return offerings.First(x => x.IsCurrent);
    }
}
