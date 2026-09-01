using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of resolving the user's store storefront country code (ISO 3166-1 alpha-2).
/// </summary>
public class StorefrontResultDto : DataResult<string, PurchaseErrorStatus>
{
}
