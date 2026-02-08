[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/kebechet)

# Maui.RevenueCat.InAppBilling

[![NuGet Version](https://img.shields.io/nuget/v/Kebechet.Maui.RevenueCat.InAppBilling)](https://www.nuget.org/packages/Kebechet.Maui.RevenueCat.InAppBilling/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Kebechet.Maui.RevenueCat.InAppBilling)](https://www.nuget.org/packages/Kebechet.Maui.RevenueCat.InAppBilling/)
![Last updated (main)](https://img.shields.io/github/last-commit/Kebechet/Maui.RevenueCat.InAppBilling/main?label=last%20updated)
[![Twitter](https://img.shields.io/twitter/url/https/twitter.com/samuel_sidor.svg?style=social&label=Follow%20samuel_sidor)](https://x.com/samuel_sidor)

A .NET MAUI wrapper library for [RevenueCat](https://www.revenuecat.com/) in-app purchases. Provides a unified C# API that abstracts away the need for you to use platform-specific code from [Android](https://github.com/Kebechet/Maui.RevenueCat.InAppBilling/tree/main/src/Maui.RevenueCat.Android) and [iOS](https://github.com/Kebechet/Maui.RevenueCat.InAppBilling/tree/main/src/Maui.RevenueCat.iOS) native bindings.

## Features

- Unified API for iOS and Android in-app purchases
- Subscription and one-time purchase support
- User authentication (anonymous and identified users)
- Subscription status and entitlement checking
- Trial/intro discount eligibility
- Subscriber attributes management
- Stub implementations for Windows and MacCatalyst (for development convenience)

## Installation

```bash
dotnet add package Kebechet.Maui.RevenueCat.InAppBilling
```

## Quick Start

### 1. Register the service

In your `MauiProgram.cs`:

```csharp
builder.Services.AddRevenueCatBilling();
```

### 2. Inject and initialize

In `App.xaml.cs`, inject `IRevenueCatBilling` and initialize in `OnStart()`:

```csharp
public partial class App : Application
{
    private readonly IRevenueCatBilling _revenueCat;

    public App(IRevenueCatBilling revenueCat)
    {
        InitializeComponent();
        _revenueCat = revenueCat;
    }

    protected override void OnStart()
    {
        var revenueCatApiKey = string.Empty;

#if __ANDROID__
        revenueCatApiKey = "<your-android-api-key>";
#elif __IOS__
        revenueCatApiKey = "<your-ios-api-key>";
#endif

        _revenueCat.Initialize(revenueCatApiKey);
        base.OnStart();
    }
}
```

> **Important**: Initialize must be called in `OnStart()`, not in the constructor.

## API Reference

### Initialization & State

| Method | Description |
|--------|-------------|
| `Initialize(string apiKey)` | Initialize RevenueCat with your platform-specific API key |
| `IsInitialized()` | Check if the SDK has been initialized |
| `IsAnonymous()` | Check if current user is anonymous |
| `GetAppUserId()` | Get the current user ID |

### Offerings & Products

| Method | Description |
|--------|-------------|
| `GetOfferings(bool forceRefresh = false)` | Fetch available offerings and packages |
| `CheckTrialOrIntroDiscountEligibility(List<string> identifiers)` | Check eligibility for trials/intro pricing |

### Purchases

| Method | Description |
|--------|-------------|
| `PurchaseProduct(PackageDto package)` | Initiate a purchase flow |
| `GetActiveSubscriptions()` | Get list of active subscription identifiers |
| `GetAllPurchasedIdentifiers()` | Get all purchased product identifiers |
| `GetPurchaseDateForProductIdentifier(string productSku)` | Get purchase date for a specific product |
| `RestoreTransactions()` | Restore previous purchases |

### User Management

| Method | Description |
|--------|-------------|
| `Login(string appUserId)` | Log in an identified user |
| `Logout()` | Log out and create anonymous user |
| `GetCustomerInfo()` | Get current customer info and entitlements |
| `GetManagementSubscriptionUrl()` | Get URL for subscription management |

### Subscriber Attributes

| Method | Description |
|--------|-------------|
| `SetEmail(string email)` | Set user's email |
| `SetDisplayName(string name)` | Set user's display name |
| `SetPhoneNumber(string phone)` | Set user's phone number |
| `SetAttributes(IDictionary<string, string> attributes)` | Set custom attributes |

## Example: Complete Purchase Flow

```csharp
public class PurchaseService
{
    private readonly IRevenueCatBilling _revenueCat;

    public PurchaseService(IRevenueCatBilling revenueCat)
    {
        _revenueCat = revenueCat;
    }

    public async Task<bool> PurchaseSubscription()
    {
        var offerings = await _revenueCat.GetOfferings();
        if (offerings.Count == 0)
            return false;

        var defaultOffering = offerings.FirstOrDefault(o => o.IsCurrent);
        var monthlyPackage = defaultOffering?.AvailablePackages
            .FirstOrDefault(p => p.Identifier == "monthly");

        if (monthlyPackage == null)
            return false;

        var result = await _revenueCat.PurchaseProduct(monthlyPackage);

        if (result.IsSuccess)
        {
            // Purchase successful
            return true;
        }

        if (result.ErrorStatus == PurchaseErrorStatus.PurchaseCancelledError)
        {
            // User cancelled - not an error
            return false;
        }

        // Handle other errors
        Console.WriteLine($"Purchase failed: {result.ErrorStatus}");
        return false;
    }

    public async Task<bool> HasActiveSubscription(string entitlementId)
    {
        var customerInfo = await _revenueCat.GetCustomerInfo();
        return customerInfo?.Entitlements
            .Any(e => e.Identifier == entitlementId) ?? false;
    }
}
```

## Platform Support

| Platform | Support |
|----------|---------|
| Android | Full implementation |
| iOS | Full implementation |
| Windows | Stub (returns defaults) |
| MacCatalyst | Stub (returns defaults) |

Stub implementations return:
- `true` for boolean methods
- Empty collections for list methods
- `string.Empty` for string methods
- `null` for nullable types

This allows you to build and test on Windows/Mac without platform conditionals.

## Error Handling

The library follows a non-throwing approach for runtime errors:

- **Exceptions are thrown only** for developer mistakes (e.g., calling methods before `Initialize()`)
- **Runtime errors** (network issues, store problems, etc.) return:
  - `ErrorStatus` in result DTOs (e.g., `PurchaseResultDto.ErrorStatus`)
  - Empty collections for list returns
  - `null` for nullable types

This design ensures your app never crashes due to store-related issues.

### Common Error Codes

| Error | Description |
|-------|-------------|
| `PurchaseCancelledError` | User cancelled the purchase |
| `StoreProblemError` | Issue with the app store |
| `NetworkError` | Network connectivity issue |
| `ProductAlreadyPurchasedError` | Product was already purchased |
| `PaymentPendingError` | Payment is pending (e.g., awaiting approval) |

See [PurchaseErrorStatus](src/Maui.RevenueCat.InAppBilling/Enums/PurchaseErrorStatus.cs) for the complete list.

## Credits

- Native bindings for [Android](https://github.com/Kebechet/Maui.RevenueCat.InAppBilling/tree/main/src/Maui.RevenueCat.Android) and [iOS](https://github.com/Kebechet/Maui.RevenueCat.InAppBilling/tree/main/src/Maui.RevenueCat.iOS) inspired by [thisisthekap](https://github.com/thisisthekap)'s Xamarin bindings
- Abstraction layer based on [RevenueCatXamarin](https://github.com/BillFulton/RevenueCatXamarin) by [BillFulton](https://github.com/BillFulton)

## Contributing

Feel free to create an issue or pull request. For major changes, please open an issue first to discuss your proposal - large PRs without prior discussion may be rejected.

## License

This project is licensed under the [MIT License](LICENSE.txt).
