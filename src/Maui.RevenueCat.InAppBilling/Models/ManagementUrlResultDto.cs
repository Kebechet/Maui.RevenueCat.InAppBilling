using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Result of resolving the store's subscription-management URL.
/// A success with a null <see cref="ManagementUrl"/> means the user has no
/// store-managed subscription - that is distinct from an <see cref="ErrorStatus"/>.
/// </summary>
public sealed record ManagementUrlResultDto
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

    /// <summary>
    /// The store's subscription-management URL, or null when the user has no
    /// store-managed subscription. Only meaningful when <see cref="IsSuccess"/> is true.
    /// </summary>
    public string? ManagementUrl { get; set; }
}
