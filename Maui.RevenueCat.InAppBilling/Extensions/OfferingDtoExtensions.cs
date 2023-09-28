using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Extensions;
public static partial class OfferingDtoExtensions
{
    public static OfferingDto? Current(this IList<OfferingDto> offers) => offers.FirstOrDefault(x => x.Current);
}
