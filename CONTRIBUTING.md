# Contributing

Bug reports, fixes and improvements are welcome. This file covers what the repository accepts, how to propose a change, and how to build and test it.

## Scope

The repository ships three NuGet packages:

| Package | Project | What it is |
|:--|:--|:--|
| `Kebechet.Maui.RevenueCat.InAppBilling` | `src/Maui.RevenueCat.InAppBilling` | The cross-platform wrapper that apps reference |
| `Kebechet.Maui.RevenueCat.Android` | `src/Maui.RevenueCat.Android` | Bindings to RevenueCat's Android SDK |
| `Kebechet.Maui.RevenueCat.iOS` | `src/Maui.RevenueCat.iOS` | Bindings to RevenueCat's iOS SDK, also used on Mac Catalyst |

RevenueCat's UI module (RevenueCatUI: paywalls, Customer Center) is out of scope. Bind and publish it as your own packages on top of the bindings here. RevenueCat's native modules must share one SDK version, so reference the binding with an exact version range (`[10.16.1]`) rather than a minimum (`10.16.1`): a mismatch then fails at restore instead of at runtime.

## Proposing a change

- **Bug fixes and small improvements:** open a pull request directly.
- **Bigger changes** (new or changed public API, a behaviour change, anything breaking): opening an issue first is highly recommended, though not required. It settles whether the change fits before you spend time on it. A large pull request without prior discussion may be declined.
- **Updating to a new RevenueCat SDK version:** open an issue asking for it. The maintainer runs the update, because the iOS bindings are generated in CI with a pinned Xcode and Objective Sharpie, and all three packages are published by workflows that use the repository's secrets. Please don't send bindings you generated locally.

## Pull requests

Every pull request needs a description: what changes and why, how it was tested (which platforms, on a device or a simulator/emulator), and whether it breaks anything for consumers. Without an issue, the description is the only place the reasoning lives, and the release notes are written from it. The pull request template asks for exactly this.

Keep one concern per pull request. Pull requests are merged with a merge commit, so every commit lands in `main` as written - see [Commit messages](#commit-messages).

## Setup

- **.NET 10 SDK.** The test projects target `net10.0`; the packages target `net9.0-*`, which the .NET 10 SDK also builds.
- **MAUI workloads:**
  ```bash
  dotnet workload install maui-android maui-ios maui-maccatalyst maui-windows
  ```
- **JDK 17** for the Android binding.
- **A Mac with Xcode** for anything iOS or Mac Catalyst.

Solutions are `.slnx` (XML format), not `.sln`:
- `src/Maui.RevenueCat.InAppBilling.slnx` - main solution; the `/tests/` folder groups `BindingLinkerCheck` and `Postprocess.Tests`
- `demo/DemoApp/DemoApp.slnx` - demo solution

### Build commands

```bash
# Build the wrapper
dotnet build src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj

# Build in Release mode
dotnet build src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj -c Release

# Create the NuGet package
dotnet pack src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj -o ./artifacts

# Restore NuGet packages
dotnet restore src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj --ignore-failed-sources
```

## Where a change goes

Most changes only touch the wrapper, `src/Maui.RevenueCat.InAppBilling`:

- **Public API:** `Services/IRevenueCatBilling.cs`, with its DTOs in `Models/`.
- **Platform code:** partial classes in `Platforms/Android/` and `Platforms/iOS/`, with the native-to-DTO conversions in each platform's `Extensions/` folder. Mac Catalyst has no folder of its own: it compiles the iOS sources.
- **Stubs:** `Platforms/Windows/` and `PlatformsStandard/` return default values and report success, so an app builds and runs on Windows without platform conditionals. Keep them that way; don't turn a stub into an error path.

A public API change touches every implementation above, the README, and usually the demo harness (`demo/DemoApp/DemoApp.Harness`) and its tests.

The library throws only when the calling developer makes a mistake, such as not calling `Initialize`. Store, network and user failures come back as a result with an error status, never as an exception - see [Error Handling](README.md#error-handling).

The binding projects document how they were created in their own READMEs ([Android](src/Maui.RevenueCat.Android/README.md), [iOS](src/Maui.RevenueCat.iOS/README.md)). The [Architecture section of CLAUDE.md](CLAUDE.md#architecture) goes deeper into the wrapper and the iOS binding pipeline.

## Testing

### Unit tests

CI runs both on every pull request:

```bash
dotnet test tests/DemoHarness.Tests/DemoHarness.Tests.csproj -c Release
dotnet test tests/Postprocess.Tests/Postprocess.Tests.csproj -c Release
```

`DemoHarness.Tests` restores the binding versions the wrapper references. When one of them is not on nuget.org yet, pack the bindings into the local feed first (see below), as CI does.

### On a device

Run `demo/DemoApp` on every platform your change touches, and name them in the pull request. The demo starts against RevenueCat's [Test Store](https://www.revenuecat.com/docs/test-and-launch/sandbox/test-store), so purchases work without any App Store Connect or Play Console products. Its harness checks every non-interactive API method; purchase, restore, login and logout are driven from the UI.

### Binding changes: test them as a real package

Test a binding change the way consumers get it: as a NuGet package consumed through the wrapper and exercised in the demo, NOT as a `ProjectReference`. A `ProjectReference` skips package restore and the embedding of the native jar/aar, so a binding that is broken as a package still builds green that way.

1. **Create a `nuget.config` in the repository root** that adds a `local` source. It is not committed:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
       <packageSources>
           <clear />
           <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
           <add key="local" value="./local-nuget" />
       </packageSources>
       <packageSourceMapping>
           <packageSource key="nuget.org">
               <package pattern="*" />
               <package pattern="Kebechet.Maui.RevenueCat.*" />
           </packageSource>
           <packageSource key="local">
               <package pattern="Kebechet.Maui.RevenueCat.*" />
           </packageSource>
       </packageSourceMapping>
   </configuration>
   ```
   The `Kebechet.Maui.RevenueCat.*` family is mapped to BOTH sources, so a locally packed version resolves from `local-nuget/` while everything else still restores from nuget.org. Packed `.nupkg` files are gitignored.
2. **Bump the binding `<Version>`** (e.g. `10.1.2.0` → `10.1.2.1`, the `<upstream>.<binding-fix>` scheme). A new version sidesteps the global package cache, so no `rm -rf ~/.nuget/packages/...` is needed.
3. **Pack into the local feed:**
   ```bash
   dotnet pack src/Maui.RevenueCat.Android/Maui.RevenueCat.Android.csproj -c Release -o local-nuget
   ```
4. **Point the wrapper's `PackageReference` at the new version** (`src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj`, in the per-TFM `ItemGroup`).
5. **Rebuild and run the demo** (`demo/DemoApp`) on the target platform. The root `nuget.config` is an ancestor of `demo/`, so the local feed is picked up automatically; the demo references the wrapper via `ProjectReference`. Inspect the packed `.nuspec` to confirm the intended dependency change (`unzip -p local-nuget/<pkg>.nupkg '*.nuspec'`).

(Same-version re-pack is still possible by clearing `~/.nuget/packages/<lowercased.id>/<version>` to force re-extract, but bumping is cleaner.)

Contributors: leave the version changes from steps 2 and 4 out of the pull request. The maintainer sets versions at release.

## Versions and release notes

- **Bindings** follow the RevenueCat SDK they wrap, as `<upstream>.<binding-fix>`: `10.16.1.0` is the first binding of RevenueCat 10.16.1, `10.16.1.1` the first fix to it.
- **The wrapper** uses semantic versioning: a breaking change bumps the major version.
- **`<Version>` and `<PackageReleaseNotes>`** are set by the maintainer in a `release:` commit when publishing, and `publish-wrapper.yml` turns the release notes into the GitHub release. Leave both unchanged in a pull request and describe user-visible changes in its description instead.

## Code style

`.editorconfig` sets the formatting (spaces, CRLF line endings). On top of that:

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

### Emptiness Checks
Prefer LINQ over `Count` comparisons — `.Any()` reads better and works on any `IEnumerable<T>`,
not just types that expose a count:
```csharp
// Correct
if (items.Any())
if (!items.Any())

// Incorrect
if (items.Count != 0)
if (items.Count == 0)
```
Inside `Maui.RevenueCat.InAppBilling` itself, prefer `IsNullOrEmpty()` from
`Kebechet.Extensions.IsNullOrEmpty` (namespace `IsNullOrEmpty.Extensions`) when the check also
covers null. That package is referenced with `<PrivateAssets>compile</PrivateAssets>`, so it does
**not** flow to `DemoApp.Harness`, the demo app or the tests — use `.Any()` there.

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

## Commit messages

Commits follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/): `<type>(<scope>)!: <description>`.

- **type:** `feat`, `fix`, `refactor`, `docs`, `test`, `style`, `ci`, `build` or `chore`. `release` is reserved for the maintainer's version commits.
- **scope** (optional): the area touched, e.g. `android`, `ios`, `android-binding`, `ios-binding`, `demo`, `postprocess`.
- **`!`** marks a change that breaks consumers.
- **description:** imperative mood, lowercase, no trailing period.

Examples from the history:

```text
feat!: let GetCustomerInfo take a cache fetch policy
docs: document how to enable RevenueCat debug logs
```
