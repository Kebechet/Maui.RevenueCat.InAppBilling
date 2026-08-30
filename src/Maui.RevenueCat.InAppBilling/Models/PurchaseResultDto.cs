using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of a purchase: the refreshed customer info like every other billing call, plus the
/// <see cref="Transaction"/> that only a purchase produces.
/// </summary>
/// <remarks>
/// Named with the <c>Dto</c> suffix because the Android binding already exposes a
/// <c>Com.Revenuecat.Purchases.PurchaseResult</c>, which an unsuffixed name would collide with.
/// </remarks>
public class PurchaseResultDto : CustomerInfoResultDto
{
    public StoreTransactionDto? Transaction { get; init; }
}
