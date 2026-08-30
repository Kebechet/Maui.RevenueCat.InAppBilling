using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of fetching the configured offerings and their packages.
/// </summary>
public class OfferingsResultDto : DataResult<List<OfferingDto>, PurchaseErrorStatus>
{
}
