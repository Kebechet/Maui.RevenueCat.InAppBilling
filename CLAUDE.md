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

### Solution layout
Solutions are `.slnx` (XML format), not `.sln`:
- `src/Maui.RevenueCat.InAppBilling.slnx` — main solution; the `/tests/` folder groups `BindingLinkerCheck` and `Postprocess.Tests`
- `demo/DemoApp/DemoApp.slnx` — demo solution

### Local NuGet feed for binding development
`nuget.config` at repo root defines a `local` source mapped to `./local-nuget`. The `Kebechet.Maui.RevenueCat.*` family is mapped (via `packageSourceMapping`) to BOTH `local` and `nuget.org`, so a locally-packed version resolves from `local-nuget/` while everything else (and CI, which has no local packages) still restores from nuget.org. `local-nuget/` is `.gitignored`.

To verify a binding change end-to-end before publishing — i.e. consumed as a real NuGet package through the wrapper and exercised in the demo, NOT a `ProjectReference` (which skips package restore and native jar/aar embedding):

1. **Bump the binding `<Version>`** (e.g. `10.1.2.0` → `10.1.2.1`, the `<upstream>.<binding-fix>` scheme). A new version sidesteps the global package cache, so no `rm -rf ~/.nuget/packages/...` is needed.
2. **Pack into the local feed:**
   ```bash
   dotnet pack src/Maui.RevenueCat.Android/Maui.RevenueCat.Android.csproj -c Release -o local-nuget
   ```
3. **Point the wrapper's `PackageReference` at the new version** (`src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj`, in the per-TFM `ItemGroup`).
4. **Rebuild + run the demo** (`demo/DemoApp`) on the target platform. The repo-root `nuget.config` is an ancestor of `demo/`, so the local feed is picked up automatically; the demo references the wrapper via `ProjectReference`. Inspect the packed `.nuspec` to confirm the intended dependency change (`unzip -p local-nuget/<pkg>.nupkg '*.nuspec'`).

(Same-version re-pack is still possible by clearing `~/.nuget/packages/<lowercased.id>/<version>` to force re-extract, but bumping is cleaner.)

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

### iOS Binding Regeneration Pipeline

The iOS binding's `ApiDefinitions.cs` / `StructsAndEnums.cs` are generated automatically rather than hand-maintained. Three pieces work together:

1. **Workflow** — `.github/workflows/generate-ios-bindings.yml` (manual dispatch). Runs on `macos-latest`: installs Objective Sharpie, checks out `RevenueCat/purchases-ios` at the requested tag, writes `Local.xcconfig` with `SWIFT_ACTIVE_COMPILATION_CONDITIONS = $(inherited) BYPASS_SIMULATED_STORE_RELEASE_CHECK`, runs `xcodebuild archive` + `xcodebuild -create-xcframework` to build the xcframework from source, verifies the bypass via `nm -gU` symbol check, then `sharpie bind -framework ...` and post-processes. Building from source (vs. downloading the pre-built `RevenueCat.xcframework.zip` release asset) is what makes RevenueCat Test Store API keys (`test_…`) usable in Release builds — the pre-built asset is always Release-configured and triggers the `#if !DEBUG && !BYPASS_SIMULATED_STORE_RELEASE_CHECK` guard in `Sources/Purchasing/Configuration.swift`, force-closing consumer apps. Mirrors `RevenueCat/purchases-kmp`'s `kn-core/build.gradle.kts`. See issue #116.

2. **Post-processor** — `.github/scripts/postprocess_ios_bindings.cs` (.NET 10 file-based script). A pipeline of deterministic passes applies every cleanup the iOS binding README documents (strip `[Verify]`, collapse multi-line `//` comments, normalize Cocoa acronym casing `NSURL`→`NSUrl`, drop device-specific availability attrs including combined `[Watch (...), TV (...), Mac (...), iOS (...)]` lines, drop `[Protocol]`-only types, drop linker-risk symbols, merge `_RevenueCat_Swift_NNNN` suffix interfaces into their canonical counterparts, add `INativeObject` to `NSDictionary`/`NSArray`/`NSSet` type parameters, dedupe overloaded methods, convert block namespace to file-scoped, etc.). The detection-only `LinkerRiskSymbols` array is the authoritative list of native symbols sharpie binds but the RevenueCat dylib doesn't export — extend it when a new SDK version surfaces `Undefined symbols for architecture arm64` errors at link time.

3. **Tests + validation**:
   - `tests/Postprocess.Tests/` (xUnit, .NET 10) — each pass has a fixture test plus regression tests for previously caught bugs (multi-suffix merge corruption, indent drop, empty-obsolete-interface eating the next interface, multi-line doc comments, combined platform attrs, etc.). Runs as the first step of `generate-ios-bindings.yml` and `publish-ios.yml`.
   - `tests/BindingLinkerCheck/` — a minimal `net9.0-ios` Exe that ProjectReferences the binding and `typeof()`s every interface, forcing the iOS registrar to emit `_OBJC_CLASS_$_X` for each. Both `generate-ios-bindings.yml` (after post-processing) and `publish-ios.yml` (before pack) build it for `ios-arm64`. Catches missing `LinkerRiskSymbols` entries before they reach NuGet — a standalone binding `dotnet build` does NOT run the native linker, so `Undefined symbols` only surfaces in a consuming app build.

When bumping the binding to a new RevenueCat version: dispatch `generate-ios-bindings`, download the artifacts, drop `ApiDefinitions.cs` / `StructsAndEnums.cs` / `nativelib/RevenueCat.xcframework` into `src/Maui.RevenueCat.iOS/`, bump `<Version>` in the csproj. The version scheme is `<upstream>.<binding-fix>` (e.g. `5.72.0.0` = first binding for RevenueCat 5.72.0; `5.72.0.1` = first fix to that binding).

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

One file per source type — name it `{SourceTypeName}Extensions.cs` and put it under
`Platforms/{Platform}/Extensions/`. A new mapping for a different native type goes in its
own file; don't pile multiple unrelated converters into one file. Examples:
`StoreExtensions.cs`, `PurchasesErrorCodeExtensions.cs`, `SubscriptionPeriodExtensions.cs`,
`SubscriptionUnitExtensions.cs`.

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

### Price Localization
`PricingDto.PriceLocalized` is set by the wrapper's `PackageDtoExtensions.GetLocalizedPrice(currencyCode, price)`, NOT by the platform's `StoreProduct.LocalizedPriceString` (iOS) / `Price.Formatted` (Android). The wrapper version drops `,00` for whole-number prices (`199 Kč`, not `199,00 Kč`) and is deterministic across runs via a stable ordinal scan over `CultureInfo.GetCultures()`. Per-period prices are derived in `GetPriceFor(PriceDuration)` / `GetPriceWithCurrencyFor(PriceDuration)` from the package's base `Price` normalized to monthly and then scaled to the requested duration (`Daily` / `Weekly` / `Monthly` / `Yearly`) — no platform per-period property is exposed on `PricingDto` (the platform's per-period strings would re-introduce the `,00` regression and only matter for trial/intro pricing that the wrapper doesn't surface anyway). If you ever want byte-for-byte parity with the App Store / Play Store payment confirmation, swap `PriceLocalized` back to the platform string in `PackageArrayExtensions.cs` (iOS) / `PackageListExtensions.cs` (Android).
