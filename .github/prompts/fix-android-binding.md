# Repairing the Android binding after a native SDK bump

You are running inside the `TryBumpAndroid` workflow. `.github/scripts/bump-android.sh` has
just moved `src/Maui.RevenueCat.Android/Maui.RevenueCat.Android.csproj` onto a new
`com.revenuecat.purchases:purchases` version and `dotnet build` failed. The .NET Android SDK
downloads that AAR and its POM from Maven Central at build time, generates C# from the AAR,
and verifies that every Java dependency the POM declares is satisfied by a NuGet package the
csproj references.

## Goal

Make these commands succeed, then stop:

```
dotnet restore src/Maui.RevenueCat.Android/Maui.RevenueCat.Android.csproj
dotnet build src/Maui.RevenueCat.Android/Maui.RevenueCat.Android.csproj -c Release --no-restore
```

Start by reading `binding-build.log` in the repository root.

## What you may edit

- The `<PackageReference>` items of `src/Maui.RevenueCat.Android/Maui.RevenueCat.Android.csproj`
  (add one, or change its `Version`).
- `src/Maui.RevenueCat.Android/Transforms/*.xml` and `src/Maui.RevenueCat.Android/Additions/`.

Everything else is discarded automatically before the verifying rebuild, the wrapper,
workflows and scripts included. In the csproj, never change `<Version>`,
`<PackageReleaseNotes>`, `<SupportedOSPlatformVersion>`, the `<AndroidMavenLibrary>` pin or
any `<AndroidIgnoredJavaDependency>`: the bump set them, and if any of them differs after
you finish, ALL your csproj edits are thrown away.

Do not run git; do not create branches or commits. The workflow commits for you.

## Dependency failures (the common case)

- **`XA4242`** "Java dependency 'g:a:v' is not satisfied. Microsoft maintains the NuGet
  package 'X'": add `<PackageReference Include="X" ... />`. For the version, prefer the one
  already resolved transitively (`grep '"X/' src/Maui.RevenueCat.Android/obj/project.assets.json`);
  otherwise the lowest version whose Java artifact (second column of `nuget-versions`) is at
  least the required version.
- **`XA4241`** "Java dependency 'g:a:v' is not satisfied" with no package named: a package
  the csproj already references provides `g:a` but at a lower version. Find it (run
  `nuget-versions` on the references whose names match `g:a`; the second column shows which
  artifact each binds) and raise it to the lowest version that satisfies `v`.
- A version's dependency group lists the minimum versions it pulls in. When a raise needs a
  dependency above what the csproj pins, that pin has to move too (see `NU1605` below), so
  read the group before you choose.
- **`NU1605`** "Detected package downgrade: X from A to B": raise the direct reference to X to
  exactly A.
- **`NU1608`** "Detected package version outside of dependency constraint: Y requires X
  (>= a && < b) but version X c was resolved": a warning, but it means a family is split.
  Raise Y (or add it as a reference) to the version whose group matches X `c`; do not finish
  with an `NU1608` you caused.
- **Keep every family on one version, on every raise, including one an `XA` error forced:**
  every `Xamarin.AndroidX.Lifecycle.*`, `Xamarin.Kotlin.StdLib` with
  `Xamarin.Kotlin.Parcelize.Runtime`, `Xamarin.KotlinX.Coroutines.Core` with `.Android`,
  `Xamarin.AndroidX.Fragment` with `.Fragment.Ktx`, and any `X` with its `X.Ktx`. When an
  error needs one member at version v and another member must move higher, the whole family
  goes to the higher one. This rule wins over "lowest version" below.
- Look versions up with
  `dotnet run .github/scripts/bump.cs -- nuget-versions <package-id> net9.0-android`, one
  package per call. It is the only way to reach nuget.org from here: `curl` and every other
  network command are blocked, and so are shell loops, so do not try them. It prints one line
  per listed stable version, oldest first: the version, the Java artifact it binds
  (`g:a:v`, what `XA4241`/`XA4242` compare against) and the dependency group a
  `net9.0-android` project resolves to, with that group's minimum dependency versions. A
  version marked `NOT usable from net9.0-android` ships only for a newer .NET: never pick it,
  however close its Java version is. The output is long for big packages; append
  `| tail -20` to see the newest versions only.
- Raise versions only as far as an error demands. Every raise reaches every app that uses
  this binding, so the smallest version that builds is the right one.
- Keep the existing XML comments in the csproj; they record why a reference exists
  (`Fragment.Ktx`, `SavedState`, the `Lifecycle.*.Ktx` lines, poko). Put a new reference next
  to its group, with no comment of its own unless the reason is not obvious.

Restore errors stop the build before generation, so fix them first and re-run; generation
errors may only appear afterwards.

## Generator failures

`BG####` / `CS####` errors name the Java type or member at fault:

- **Java overloads erasing to the same C# signature**: rename one with
  `<attr path="..." name="managedName">NewName</attr>` in `Transforms/Metadata.xml`.
- **Erased Java generics the generator cannot re-implement** (`CS0534` / `CS0115`): remove the
  specific class with `remove-node`, only when the wrapper does not use it.
- **Kotlin companions / `DefaultImpls` breaking generation**: `remove-node` the specific
  nested class, never a whole package.

Keep the surface the wrapper uses bound: everything under `com.revenuecat.purchases` that
`src/Maui.RevenueCat.InAppBilling/Platforms/Android/` calls (`grep -rhoE 'Com\.Revenuecat\.[A-Za-z.]+'`
lists it). Removing a member the wrapper uses just to make the build pass is not a fix.

## Loop

Edit -> restore + build -> read the new errors -> repeat. Stop when the build passes, or
when you are convinced the failure cannot be fixed within the files above - in that case
undo your edits.

## When done

Write `fix-summary.md` in the repository root (3-10 lines of markdown): which errors you hit,
which references you added or raised (from -> to) and why, and anything a reviewer should
double-check - in particular every AndroidX or Kotlin version you raised, because consuming
apps inherit it. It is pasted into the pull request. If you changed nothing, write one line
saying so and why.
