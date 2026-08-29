using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Result of an operation that returns customer info and can fail -
/// login, logout, restore and customer info reads all share this shape.
/// <see cref="PurchaseResultDto"/> is the purchase-flow counterpart; it additionally
/// carries the <see cref="PurchaseResultDto.Transaction"/> these operations never produce.
/// </summary>
public sealed record CustomerInfoResultDto
{
    public bool IsSuccess { get; set; }
    public bool IsError => !(ErrorStatus is null);
    public PurchaseErrorStatus? ErrorStatus { get; set; }

    /// <summary>
    /// Human-readable detail of the failure from the underlying store SDK
    /// (message, underlying error, native code). Null on success and on cancellation.
    /// <see cref="ErrorStatus"/> stays the value to branch on; this is for logs and diagnostics.
    /// </summary>
    public string? ErrorMessage { get; set; }

    public CustomerInfoDto? CustomerInfo { get; set; }
}
