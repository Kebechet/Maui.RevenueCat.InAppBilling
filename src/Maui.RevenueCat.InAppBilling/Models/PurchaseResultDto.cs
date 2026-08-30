using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of a purchase: the refreshed customer info, plus the <see cref="Transaction"/>
/// that only a purchase produces.
/// </summary>
/// <remarks>
/// A sibling of <see cref="CustomerInfoResultDto"/>, not a subclass. The two carry the same
/// payload today, but a purchase is not a kind of customer-info read - keeping them independent
/// means neither is substitutable for the other, and either can gain members without the other
/// inheriting them.
/// <para>
/// Named with the <c>Dto</c> suffix because the Android binding already exposes a
/// <c>Com.Revenuecat.Purchases.PurchaseResult</c>, which an unsuffixed name would collide with.
/// </para>
/// </remarks>
public class PurchaseResultDto : DataResult<CustomerInfoDto, PurchaseErrorStatus>
{
    public StoreTransactionDto? Transaction { get; init; }
}
