using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Services;

public interface IRevenueCatBilling
{
    void Initialize(string apiKey);
    bool IsInitialized();
    Task<List<OfferingDto>> LoadOfferings(bool forceRefresh = false);
    Task<bool> PurchaseProduct(string offeringIdentifier);
    Task<List<string>> GetActiveSubscriptions();
    Task<List<string>> GetAllPurchasedIdentifiers();
    Task<DateTime?> GetPurchaseDateForProductIdentifier(string productSku);
    Task<string> GetManagementSubscriptionUrl();
    //Purchases.SharedInstance.LogIn
    //Purchases.SharedInstance.LogOut
}
