using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of showing the store's subscription management screen.
/// </summary>
public class ManageSubscriptionsResultDto : Result<PurchaseErrorStatus>
{
}
