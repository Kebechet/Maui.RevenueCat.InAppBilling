using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of an operation that returns a list of product identifiers -
/// active subscriptions and all purchased identifiers share this shape.
/// </summary>
public class ProductIdentifiersResultDto : DataResult<List<string>, PurchaseErrorStatus>
{
}
