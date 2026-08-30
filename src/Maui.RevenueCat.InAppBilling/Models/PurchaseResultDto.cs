using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of a purchase. Carries the refreshed customer info in
/// <see cref="DataResult{TValue}.Value"/> like every other billing call, plus the
/// <see cref="Transaction"/> that only a purchase produces.
/// </summary>
/// <remarks>
/// Named with the <c>Dto</c> suffix because the Android binding already exposes a
/// <c>Com.Revenuecat.Purchases.PurchaseResult</c>, which an unsuffixed name would collide with.
/// </remarks>
public class PurchaseResultDto : DataResult<CustomerInfoDto, PurchaseErrorStatus>
{
    public StoreTransactionDto? Transaction { get; init; }
}
