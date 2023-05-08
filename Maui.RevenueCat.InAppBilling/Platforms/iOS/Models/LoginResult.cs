using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Models;

public struct LoginResult
{
    public RCCustomerInfo CustomerInfo { get; }
    public bool Created { get; }

    public LoginResult(RCCustomerInfo customerInfo, bool created)
    {
        CustomerInfo = customerInfo;
        Created = created;
    }
}
