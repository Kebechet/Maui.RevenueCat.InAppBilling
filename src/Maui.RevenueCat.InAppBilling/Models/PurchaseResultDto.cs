using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record PurchaseResultDto
{
    public bool IsSuccess { get; set; }
    public bool IsError => !(ErrorStatus is null);
    public PurchaseErrorStatus? ErrorStatus { get; set; }

    /// <summary>
    /// Human-readable detail of the failure from the underlying store SDK
    /// (message, underlying error, native code). Null on success and on user cancellation.
    /// <see cref="ErrorStatus"/> stays the value to branch on; this is for logs and diagnostics.
    /// </summary>
    public string? ErrorMessage { get; set; }

    public StoreTransactionDto? Transaction { get; set; }
    public CustomerInfoDto? CustomerInfo { get; set; }
}
