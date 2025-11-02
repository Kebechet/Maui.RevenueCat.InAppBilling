namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record StoreTransactionDto
{
    public required string ProductIdentifier { get; init; }
    public required DateTime PurchaseDate { get; init; }
    public required string TransactionIdentifier { get; init; }
    public required long Quantity { get; init; }
}
