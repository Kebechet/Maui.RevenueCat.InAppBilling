using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of an operation that resolves the current customer's info -
/// login, logout, restore and customer info reads all share this shape.
/// </summary>
public class CustomerInfoResultDto : DataResult<CustomerInfoDto, PurchaseErrorStatus>
{
}
