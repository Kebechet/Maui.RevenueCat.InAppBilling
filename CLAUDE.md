# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET MAUI cross-platform wrapper library for RevenueCat in-app billing. It provides a unified C# API that abstracts platform-specific RevenueCat SDK implementations for iOS, MacCatalyst (which shares the iOS sources) and Android, with stub implementations for Windows and unsupported platforms.

**Published as**: `Kebechet.Maui.RevenueCat.InAppBilling` on NuGet

## Contributing Workflow

Setup, build commands, testing (including the local NuGet feed for binding changes), versioning, code style and commit messages live in `CONTRIBUTING.md`, shared with human contributors and imported here:

@CONTRIBUTING.md

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
   - `PlatformsStandard/RevenueCatBillingStandard.cs` - Stub for unsupported platforms
   - MacCatalyst has **no** file of its own. The MAUI SDK auto-excludes `Platforms/iOS` for the
     maccatalyst TFM, so the `_IncludeIosSourcesForMacCatalyst` target in `Maui.RevenueCat.csproj`
     re-adds them: maccatalyst compiles the real iOS implementation. Closes #48.

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

1. **Workflow** — `.github/workflows/generate-ios-bindings.yml` (manual dispatch). Runs on `macos-latest`: installs Objective Sharpie, checks out `RevenueCat/purchases-ios` at the requested tag, writes `Local.xcconfig` with `SWIFT_ACTIVE_COMPILATION_CONDITIONS = $(inherited) BYPASS_SIMULATED_STORE_RELEASE_CHECK` (picked up because upstream `Global.xcconfig` does `#include? "Local.xcconfig"`), asserts the flag landed in the target's resolved `SWIFT_ACTIVE_COMPILATION_CONDITIONS` via `xcodebuild -showBuildSettings`, runs `xcodebuild archive` + `xcodebuild -create-xcframework` to build the xcframework from source, then `sharpie bind -framework ...` and post-processes. Building from source (vs. downloading the pre-built `RevenueCat.xcframework.zip` release asset) is what makes RevenueCat Test Store API keys (`test_…`) usable in Release builds — the pre-built asset is always Release-configured and triggers the `#if !DEBUG && !BYPASS_SIMULATED_STORE_RELEASE_CHECK` guard in `Sources/Purchasing/Configuration.swift`, force-closing consumer apps. Mirrors `RevenueCat/purchases-kmp`'s `kn-core/build.gradle.kts`. See issue #116.

2. **Post-processor** — `.github/scripts/postprocess_ios_bindings.cs` (.NET 10 file-based script). A pipeline of deterministic passes applies every cleanup the iOS binding README documents (strip `[Verify]`, collapse multi-line `//` comments, normalize Cocoa acronym casing `NSURL`→`NSUrl`, drop device-specific availability attrs including combined `[Watch (...), TV (...), Mac (...), iOS (...)]` lines, drop `[Protocol]`-only types, drop linker-risk symbols **together with their `[Category]`/extension interfaces** (e.g. `PaymentQueueWrapper_RevenueCat_Swift_NNNN` whose `[BaseType (typeof(PaymentQueueWrapper))]` would otherwise dangle as `CS0246`), drop bgen-unsupported `byte[]` `[Field]` properties (sharpie binds C `const unsigned char[]` like `RevenueCatVersionString` as `byte[]` → `BI1014`), extract inline nested `Action<Action<…>>` block-callback params into named `[BlockCallback]` delegates (bgen's Trampolines generator can't emit the nested `System.Action\`N` shape), merge `_RevenueCat_Swift_NNNN` suffix interfaces into their canonical counterparts, add `INativeObject` to `NSDictionary`/`NSArray`/`NSSet` type parameters, dedupe overloaded methods, convert block namespace to file-scoped, etc.). The detection-only `LinkerRiskSymbols` array is the authoritative list of native symbols sharpie binds but the RevenueCat dylib doesn't export — extend it when a new SDK version surfaces `Undefined symbols for architecture arm64` errors at link time.

3. **Tests + validation**:
   - `tests/Postprocess.Tests/` (xUnit, .NET 10) — each pass has a fixture test plus regression tests for previously caught bugs (multi-suffix merge corruption, indent drop, empty-obsolete-interface eating the next interface, multi-line doc comments, combined platform attrs, etc.). Runs as the first step of `generate-ios-bindings.yml` (`publish-ios.yml` builds only `BindingLinkerCheck`, it does not run these tests).
   - `tests/BindingLinkerCheck/` — a minimal `net9.0-ios` Exe that ProjectReferences the binding and `typeof()`s every interface, forcing the iOS registrar to emit `_OBJC_CLASS_$_X` for each. Both `generate-ios-bindings.yml` (after post-processing) and `publish-ios.yml` (before pack) build it for `ios-arm64`. Catches missing `LinkerRiskSymbols` entries before they reach NuGet — a standalone binding `dotnet build` does NOT run the native linker, so `Undefined symbols` only surfaces in a consuming app build. It sets `EnableCodeSigning=false` (empty `CodesignKey` alone does NOT skip signing on a device build) so the post-link signing step doesn't fail on CI runners without a keychain identity — the native link, which is all we validate, has already run by then.
   - **iOS toolchain pin (non-obvious):** `generate-ios-bindings.yml` runs the post-processor on **.NET 10** (required for the file-based script) but pins the BindingLinkerCheck/binding build to **.NET 9.0.101 + Xcode 16.4** — the same pair `publish-ios.yml` uses. This is mandatory: the .NET 10 `ios` workload requires Xcode 26.5, which the GitHub macOS runners don't carry (newest is ~26.3). The pin is three things together: install both SDKs, `sudo xcode-select -s /Applications/Xcode_16.4.app`, and a `global.json` (created *after* the .NET 10 steps) plus `dotnet workload install ios maui-ios --version 9.0.101` — the `--version` is essential, without it the install resolves the latest manifest (`net9.0_26.5`, needs Xcode 26.5) even under the 9.0.101 SDK. The fresh sharpie output lives in `src/Maui.RevenueCat.iOS/RevenueCatBindingOutput/`, excluded from the binding project via `<DefaultItemExcludes>` so the default `**/*.cs` glob doesn't compile it a second time (`CS0101`).

When bumping the binding to a new RevenueCat version: dispatch `generate-ios-bindings`, download the artifacts, drop `ApiDefinitions.cs` / `StructsAndEnums.cs` / `nativelib/RevenueCat.xcframework` into `src/Maui.RevenueCat.iOS/`, bump `<Version>` in the csproj. The version scheme is `<upstream>.<binding-fix>` (e.g. `5.72.0.0` = first binding for RevenueCat 5.72.0; `5.72.0.1` = first fix to that binding).

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
Windows and PlatformsStandard have dummy implementations that return empty collections and default
values to avoid platform-specific code in consuming applications. MacCatalyst is **not** a stub - it
compiles the real iOS implementation (see the Partial Class Pattern section).

The stubs return mock data and report success — that is the point: a consuming app builds and runs
on Windows without platform conditionals. Do not turn a stub into an error path.

One detail worth keeping straight: `GetStorefrontCountryCode()` returns `string.Empty`, not `null`,
matching the interface contract and both real platforms. `null` is the `DataResult` failure
sentinel, so a stub must not use it to mean "unknown".

### Price Localization
`PricingDto.PriceLocalized` is set by the wrapper's `PackageDtoExtensions.GetLocalizedPrice(currencyCode, price)`, NOT by the platform's `StoreProduct.LocalizedPriceString` (iOS) / `Price.Formatted` (Android). The wrapper version drops `,00` for whole-number prices (`199 Kč`, not `199,00 Kč`) and is deterministic across runs via a stable ordinal scan over `CultureInfo.GetCultures()`. Per-period prices are derived in `GetPriceFor(PriceDuration)` / `GetPriceWithCurrencyFor(PriceDuration)` from the package's base `Price` normalized to monthly and then scaled to the requested duration (`Daily` / `Weekly` / `Monthly` / `Yearly`) — no platform per-period property is exposed on `PricingDto` (the platform's per-period strings would re-introduce the `,00` regression and only matter for trial/intro pricing that the wrapper doesn't surface anyway). If you ever want byte-for-byte parity with the App Store / Play Store payment confirmation, swap `PriceLocalized` back to the platform string in `PackageArrayExtensions.cs` (iOS) / `PackageListExtensions.cs` (Android).
