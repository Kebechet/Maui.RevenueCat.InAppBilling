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
  otherwise the lowest version whose nuspec tag `artifact_versioned=g:a:<v>` is at least the
  required version.
- **`XA4241`** "Java dependency 'g:a:v' is not satisfied" with no package named: a package
  the csproj already references provides `g:a` but at a lower version. Find it (its nuspec
  carries `artifact=g:a`) and raise it to the lowest version that satisfies `v`.
- **`NU1605`** "Detected package downgrade: X from A to B": raise the direct reference to X to
  exactly A. Keep a family aligned when you move one member: every
  `Xamarin.AndroidX.Lifecycle.*` on one version, `Xamarin.Kotlin.StdLib` and
  `Xamarin.Kotlin.Parcelize.Runtime` on one version, `Xamarin.KotlinX.Coroutines.Core` and
  `.Android` on one version, `Xamarin.AndroidX.Fragment` and `.Fragment.Ktx` on one version.
- Look versions up with read-only requests, for example
  `curl -s https://api.nuget.org/v3-flatcontainer/<lowercase-id>/index.json` and
  `curl -s https://api.nuget.org/v3-flatcontainer/<id>/<version>/<id>.nuspec`. Check that the
  chosen version has a `net9.0-android` dependency group: the binding targets `net9.0-android`.
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
