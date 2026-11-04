# Repairing the iOS binding after a native SDK bump

You are running inside the `TryBumpIOS` workflow on a macOS runner. The workflow has built
`RevenueCat.xcframework` from source, generated bindings with Objective Sharpie and run them
through `.github/scripts/postprocess_ios_bindings.cs`, and `.github/scripts/bump-ios.sh` has
put `ApiDefinitions.cs`, `StructsAndEnums.cs` and `nativelib/RevenueCat.xcframework` into
`src/Maui.RevenueCat.iOS/` and bumped `<Version>`. Building or linking then failed.

## Goal

Make this command succeed, then stop:

```
.github/scripts/build-ios-binding.sh
```

It builds the binding on the .NET SDK and Xcode that publish it, then links
`tests/BindingLinkerCheck` for `ios-arm64` and `maccatalyst-arm64`. Do not call `dotnet build`
yourself: from the repository root it picks a different SDK whose iOS workload is not
installed, and its errors would mislead you.

Start by reading `binding-build.log` (the failure) and `manual_review.md` (what the
post-processor detected but did not edit) in the repository root. The Objective-C truth is in
`src/Maui.RevenueCat.iOS/nativelib/RevenueCat.xcframework/ios-arm64/RevenueCat.framework/Headers/`.

## What you may edit

- `.github/scripts/postprocess_ios_bindings.cs` and `tests/Postprocess.Tests/*.cs` - the
  preferred place for a fix, see below.
- `src/Maui.RevenueCat.iOS/ApiDefinitions.cs` and `src/Maui.RevenueCat.iOS/StructsAndEnums.cs`.

Everything else is discarded automatically before the verifying rebuild: the csproj, the
xcframework, the wrapper, workflows and the other scripts. Never change `<Version>`; never
touch `nativelib/` or `src/Maui.RevenueCat.InAppBilling/`.

Do not run git; do not create branches or commits. The workflow commits for you.

## Fix the generator, not only its output

The bindings are regenerated from scratch on every bump. A hand edit to `ApiDefinitions.cs`
fixes this bump only - the next one hits the same failure again. So when the cause is a
pattern (a native class the framework does not export, a Sharpie construct bgen rejects),
fix it in the post-processor:

1. Extend `postprocess_ios_bindings.cs` (most often: a new entry in `LinkerRiskSymbols`).
2. Add or extend a test in `tests/Postprocess.Tests/` that pins the new behaviour, and run
   `dotnet test tests/Postprocess.Tests/Postprocess.Tests.csproj`.
3. Run `.github/scripts/regenerate-ios-binding.sh`. It re-runs the post-processor over this
   run's raw Sharpie output and overwrites `ApiDefinitions.cs` / `StructsAndEnums.cs` - so do
   this BEFORE any hand edit, never after one.
4. Run `.github/scripts/build-ios-binding.sh`.

Hand-edit `ApiDefinitions.cs` only for a one-off that no rule should generalise, and say so
in the summary.

## Known patterns

- **`Undefined symbols for architecture arm64: _OBJC_CLASS_$_X`** at link time: Sharpie bound
  an internal class the RevenueCat framework does not export. Add `X` to `LinkerRiskSymbols`;
  the post-processor drops it together with the category interfaces built on it.
- **Inline `[BlockCallback] Action<...>` / `Func<...>` parameters** that break
  `Trampolines.g.cs`: extract a named delegate (the post-processor already does this for
  nested `Action<Action<...>>`; extend that pass for a new shape).
- **`BI1014`** on a `byte[]` `[Field]`: the post-processor drops those; a new one means its
  pattern needs widening.
- **Symbol-not-found from `Name = "..."`** attribute arguments: drop the argument for that
  member.
- **Duplicate selectors / members** after the suffix-interface merge: rename one managed
  member, keep every `[Export("selector")]` string exactly as Sharpie emitted it.

Keep the surface the wrapper consumes bound: every RevenueCat type and member that
`src/Maui.RevenueCat.InAppBilling/Platforms/iOS/` calls (`RCPurchases`, `RCCustomerInfo`,
`RCStoreProduct`, `RCOfferings`, `RCPackage`, `RCEntitlementInfo`, `RCStoreTransaction`, ...).
Deleting a member the wrapper uses to make the build pass is not a fix.

## Loop

Edit -> regenerate (if the post-processor changed) -> build -> read the new errors -> repeat.
Stop when the build passes, or when the failure cannot be fixed within the files above - in
that case undo your edits.

## When done

Write `fix-summary.md` in the repository root (3-10 lines of markdown): which errors you hit,
what you changed (post-processor rule, test, or hand edit) and why, and anything a reviewer
should double-check. It is pasted into the pull request. If you changed nothing, write one
line saying so and why.
