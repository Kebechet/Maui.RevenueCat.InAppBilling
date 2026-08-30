using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of resolving the store's subscription-management URL. A success with a null
/// <see cref="DataResult{TValue}.Value"/> means the user has no store-managed subscription,
/// which is distinct from a failure.
/// </summary>
public class ManagementUrlResultDto : DataResult<string, PurchaseErrorStatus>
{
}
