using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record PurchaseResultDto
{
    public bool IsSuccess { get; set; }
    public bool IsError => !(ErrorStatus is null);
    public PurchaseErrorStatus? ErrorStatus { get; set; }
}
