using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of looking up when a product was purchased. A success with a null
/// <see cref="DataResult{TValue}.Value"/> means the product was never purchased,
/// which is distinct from a failure.
/// </summary>
public class PurchaseDateResultDto : DataResult<DateTime?, PurchaseErrorStatus>
{
}
