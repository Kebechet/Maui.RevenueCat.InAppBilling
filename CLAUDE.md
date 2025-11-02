# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET MAUI cross-platform wrapper library for RevenueCat in-app billing. It provides a unified C# API that abstracts platform-specific RevenueCat SDK implementations for iOS and Android, with stub implementations for Windows and MacCatalyst.

**Published as**: `Kebechet.Maui.RevenueCat.InAppBilling` on NuGet

## Build Commands

### Build the main wrapper package
```bash
dotnet build src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj
```

### Build in Release mode
```bash
dotnet build src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj -c Release
```

### Create NuGet package
```bash
dotnet pack src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj -o ./artifacts
```

### Restore NuGet packages
```bash
dotnet restore src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj --ignore-failed-sources
```

### Required workloads
The project requires MAUI workloads for multi-platform targeting:
```bash
dotnet workload install maui-android maui-ios maui-maccatalyst maui-windows
```

## Architecture

### Three-Layer Structure

1. **Native Bindings Layer** (separate projects)
   - `Maui.RevenueCat.Android` - Android Java bindings to RevenueCat SDK
   - `Maui.RevenueCat.iOS` - iOS Objective-C bindings to RevenueCat SDK
   - These are published as separate NuGet packages and referenced by the wrapper

2. **Abstraction/Wrapper Layer** (`Maui.RevenueCat.InAppBilling`)
   - Single multi-targeted project with platform-specific partial implementations
   - DTOs in `Models/` folder provide platform-agnostic data structures
   - `Services/IRevenueCatBilling.cs` defines the public API contract

3. **Platform-Specific Implementations** (partial classes)
   - `Platforms/iOS/RevenueCatBillingiOS.cs` - Real iOS implementation
   - `Platforms/Android/RevenueCatBillingAndroid.cs` - Real Android implementation
   - `Platforms/Windows/RevenueCatBillingWindows.cs` - Stub implementation
   - `Platforms/MacCatalyst/RevenueCatBillingMac.cs` - Stub implementation
   - `PlatformsStandard/RevenueCatBillingStandard.cs` - Stub for unsupported platforms

### Partial Class Pattern

The main service class `RevenueCatBilling` uses C# partial classes to split platform logic:

- **Base partial class** (`Services/RevenueCatBilling.cs`): Contains common logic, singleton enforcement, and partial method declarations
- **Platform partial classes** implement the partial methods with platform-specific code
- At compile time, the compiler merges the appropriate platform implementation based on target framework

### Extension Methods Pattern

Platform-specific conversions are handled via extension methods in `Platforms/{Platform}/Extensions/`:
- Convert native SDK types to platform-agnostic DTOs
- Example: `RCCustomerInfo.ToCustomerInfoDto()` converts iOS type to shared DTO
- Time conversions extracted to `JavaDateExtensions` (Android) and `NsDateExtensions` (iOS)

### StoreKit1 vs StoreKit2 Handling (iOS)

The iOS implementation supports both StoreKit1 and StoreKit2 transactions:
- Checks if `Sk1Transaction` exists (StoreKit1)
- Falls back to `TransactionIdentifier` check (StoreKit2)
- See `Platforms/iOS/RevenueCatBillingiOS.cs` `PurchaseProduct` method

## Code Style Guidelines

### Comments
Comments should only be used when something is used for a non-obvious reason or is hard to understand. Do not add comments for self-explaining code.

### Empty Collections
Use collection expression syntax `[]` instead of `new()` for empty collections:
```csharp
// Correct
return [];

// Incorrect
return new();
```

### Extension Methods
Extract transformation logic to extension methods rather than inline:
```csharp
// Correct
return storeTransaction.PurchaseTime.ToDateTime();

// Incorrect
var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
return epoch.AddMilliseconds(storeTransaction.PurchaseTime);
```

### Shared Constants
Extract repeated values to static readonly fields:
```csharp
// Correct
private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
```

## Exception Handling Strategy

From README.md:
- Library throws exceptions **only** when developer makes a mistake (e.g., not calling Initialize)
- For runtime errors (corrupted state, API failures), methods return:
  - `ErrorCode` in output DTOs (e.g., `PurchaseResultDto.ErrorStatus`)
  - Default/empty values for collections
  - `null` for nullable types
- Never crash the app for user-related or network issues

## Key Implementation Details

### Singleton Pattern
`RevenueCatBilling` enforces a single instance via static flag in constructor. Creating multiple instances throws `InvalidOperationException`.

### Initialization
Must be called in `App.xaml.cs` `OnStart()` method (not in constructor) with platform-specific API keys using conditional compilation directives.

### Transaction DTOs
`StoreTransactionDto` provides unified transaction information across platforms:
- **iOS**: Maps from `RCStoreTransaction` (has dedicated properties)
- **Android**: Maps from `StoreTransaction` (uses `PurchaseToken` as identifier, derives quantity from `ProductIds.Count`)

### Stub Implementations
Windows, MacCatalyst, and PlatformsStandard have dummy implementations that return empty collections and default values to avoid platform-specific code in consuming applications.
