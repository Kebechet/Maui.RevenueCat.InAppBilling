using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of asking the store whether the device is allowed to make payments.
/// </summary>
public class CanMakePaymentsResultDto : DataResult<bool, PurchaseErrorStatus>
{
}
