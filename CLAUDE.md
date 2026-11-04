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

1. **Workflow** — `.github/workflows/try-bump-ios.yml` (daily schedule, or dispatch with a version). Runs on `macos-15`: installs Objective Sharpie, checks out `RevenueCat/purchases-ios` at the target tag, writes `Local.xcconfig` with `SWIFT_ACTIVE_COMPILATION_CONDITIONS = $(inherited) BYPASS_SIMULATED_STORE_RELEASE_CHECK` (picked up because upstream `Global.xcconfig` does `#include? "Local.xcconfig"`), asserts the flag landed in the target's resolved `SWIFT_ACTIVE_COMPILATION_CONDITIONS` via `xcodebuild -showBuildSettings`, runs `xcodebuild archive` + `xcodebuild -create-xcframework` to build the xcframework from source, then `sharpie bind -framework ...` and post-processes. Building from source (vs. downloading the pre-built `RevenueCat.xcframework.zip` release asset) is what makes RevenueCat Test Store API keys (`test_…`) usable in Release builds — the pre-built asset is always Release-configured and triggers the `#if !DEBUG && !BYPASS_SIMULATED_STORE_RELEASE_CHECK` guard in `Sources/Purchasing/Configuration.swift`, force-closing consumer apps. Mirrors `RevenueCat/purchases-kmp`'s `kn-core/build.gradle.kts`. See issue #116.

2. **Post-processor** — `.github/scripts/postprocess_ios_bindings.cs` (.NET 10 file-based script). A pipeline of deterministic passes applies every cleanup the iOS binding README documents (strip `[Verify]`, collapse multi-line `//` comments, normalize Cocoa acronym casing `NSURL`→`NSUrl`, drop device-specific availability attrs including combined `[Watch (...), TV (...), Mac (...), iOS (...)]` lines, drop `[Protocol]`-only types, drop linker-risk symbols **together with their `[Category]`/extension interfaces** (e.g. `PaymentQueueWrapper_RevenueCat_Swift_NNNN` whose `[BaseType (typeof(PaymentQueueWrapper))]` would otherwise dangle as `CS0246`), drop bgen-unsupported `byte[]` `[Field]` properties (sharpie binds C `const unsigned char[]` like `RevenueCatVersionString` as `byte[]` → `BI1014`), extract inline nested `Action<Action<…>>` block-callback params into named `[BlockCallback]` delegates (bgen's Trampolines generator can't emit the nested `System.Action\`N` shape), merge `_RevenueCat_Swift_NNNN` suffix interfaces into their canonical counterparts, add `INativeObject` to `NSDictionary`/`NSArray`/`NSSet` type parameters, dedupe overloaded methods, convert block namespace to file-scoped, etc.). The detection-only `LinkerRiskSymbols` array is the authoritative list of native symbols sharpie binds but the RevenueCat dylib doesn't export — extend it when a new SDK version surfaces `Undefined symbols for architecture arm64` errors at link time.

3. **Tests + validation**:
   - `tests/Postprocess.Tests/` (xUnit, .NET 10) — each pass has a fixture test plus regression tests for previously caught bugs (multi-suffix merge corruption, indent drop, empty-obsolete-interface eating the next interface, multi-line doc comments, combined platform attrs, etc.). Runs early in `try-bump-ios.yml`, and again after a repair that changed the post-processor (`publish-ios.yml` builds only `BindingLinkerCheck`, it does not run these tests).
   - `tests/BindingLinkerCheck/` — a minimal `net9.0-ios` Exe that ProjectReferences the binding and `typeof()`s every bound class, forcing the iOS registrar to emit `_OBJC_CLASS_$_X` for each. The `typeof()` block between the `<binding-types>` markers in `Program.cs` is generated, not hand-kept: `build-ios-binding.sh` rewrites it from the freshly built binding (`bump.cs linker-check-types`: every public class with `[Register]`). The hand-kept list had gone stale (5.83.1's three new classes were never added, and 5.102.0 removed nine it named, which broke its compile). Both `try-bump-ios.yml` (through `.github/scripts/build-ios-binding.sh`, after post-processing) and `publish-ios.yml` (before pack) build it for `ios-arm64` and `maccatalyst-arm64`. Catches missing `LinkerRiskSymbols` entries before they reach NuGet — a standalone binding `dotnet build` does NOT run the native linker, so `Undefined symbols` only surfaces in a consuming app build. It sets `EnableCodeSigning=false` (empty `CodesignKey` alone does NOT skip signing on a device build) so the post-link signing step doesn't fail on CI runners without a keychain identity — the native link, which is all we validate, has already run by then.
   - **iOS toolchain pin (non-obvious):** `try-bump-ios.yml` runs the file-based scripts (post-processor, `bump.cs`) on **.NET 10** but builds the binding, `BindingLinkerCheck` and the wrapper on **.NET 9.0.101 + Xcode 16.4** — the same pair `publish-ios.yml` uses, and the Xcode the shipped 5.83.1 framework was archived with (`DTXcode` 1640). This is mandatory: the .NET 10 `ios` workload requires a newer Xcode than Objective Sharpie can parse the SDK of. The pin is: install both SDKs, `sudo xcode-select -s /Applications/Xcode_16.4.app`, `dotnet workload install ios maccatalyst maui-ios maui-maccatalyst --version 9.0.101` — the `--version` is essential, without it the install resolves the latest manifest (`net9.0_26.5`, needs Xcode 26.5) even under the 9.0.101 SDK — and a `global.json` that lives in its own directory (`$SDK9_DIR`), NOT the repository root: every .NET 9 build runs from there with absolute paths (`build-ios-binding.sh`, `build-wrapper-against.sh` via `DOTNET_BUILD_DIR`), so the .NET 10 scripts keep working in the same job. The fresh Xcode and Sharpie output lives under `$RUNNER_TEMP`, never inside the binding project.

### Native SDK bumps (bump bot)

`try-bump-android.yml` and `try-bump-ios.yml` run daily and open one pull request per platform, `bump/<platform>-<version>`, authored by the `kebechet-bump-bot` GitHub App (secrets `BUMP_BOT_APP_ID` / `BUMP_BOT_APP_PRIVATE_KEY`; an App token, because pushes made with `GITHUB_TOKEN` raise no events and `unit-tests.yml` would never run on the PR). It is the same pipeline as in `Kebechet/Maui.MicrosoftClarity` and `Kebechet/Maui.Facebook.Core`, adapted here:

- **The PR is binding-only.** The wrapper's `PackageReference` moves onto the new binding in a separate change, once the binding is live on nuget.org; `open-bump-pr.sh` refuses to commit if the wrapper csproj differs from HEAD. `build-wrapper-against.sh` still compiles the wrapper against the freshly packed binding (wrapper csproj rewritten for the build and restored byte-for-byte).
- **Green PR = ready for review; anything else opens as a draft** labelled `binding-broken`, `wrapper-broken`, `min-os-raised` or `upstream-breaking`. `upstream-breaking` is a new major, or any release in the range with a "Breaking Changes" section in RevenueCat's notes. A green PR closes older bump PRs of the same platform.
- **Repairs:** when the binding does not build and `CLAUDE_CODE_OAUTH_TOKEN` is set, Claude Code repairs it following `.github/prompts/fix-<platform>-binding.md`. `keep-only-allowed-changes.sh` then discards edits outside an allow-list (Android: `Transforms/`, `Additions/`, the csproj's dependency list; iOS: `ApiDefinitions.cs`, `StructsAndEnums.cs`, the post-processor and its tests), and a deterministic rebuild decides. On Android the csproj values the bump owns (pin, `<Version>`, notes, floor, ignored dependencies) are compared before and after; a repair that moved any of them loses all its csproj edits.
- **Android dependencies:** the AAR's POM drives Java dependency verification (`XA4241` / `XA4242`), so a RevenueCat release that moves Play Billing, Kotlin or AndroidX fails the first build and the repair raises the `PackageReference`s. `<AndroidIgnoredJavaDependency>` pins (`group:artifact:version`) are moved onto the POM's version by the bump itself, since why a dependency is ignored does not depend on its version.
- **Release notes** (`<PackageReleaseNotes>`) are replaced, not appended: one line per upstream release since the binding live on nuget.org (not the repo pin), RevenueCat SDK section only (RevenueCatUI is not bound), without CI/tooling entries and without `@handles` (the text is also the PR body, where a handle notifies its owner).
- **Native API report:** `bump.cs api-report` diffs the public surface of the new binding assembly against the newest published binding package, and reads the wrapper's member references to list (a) what the wrapper uses that the bump removes or deprecates and (b) new members on native types the wrapper already uses - API RevenueCat added that the wrapper does not expose yet. It goes into the PR body; the untruncated version is the `api-report-<platform>` artifact.
- **Not verified by the bot:** the demo harness on a device. Run `demo/DemoApp` on the platform before publishing the binding.

For a manual bump, dispatch the workflow with `version`; `dry_run: true` runs everything and prints the PR instead of opening it. The version scheme is `<upstream>.<binding-fix>` (e.g. `5.72.0.0` = first binding for RevenueCat 5.72.0; `5.72.0.1` = first fix to that binding).

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
