#!/usr/bin/env bash
#
# Builds the iOS binding and links it the way a consuming app does, on the toolchain that
# publishes it. A plain `dotnet build` of a binding runs bgen and the C# compiler but never
# the native linker, so a native class Sharpie binds and the framework does not export only
# fails in a consumer's app ("Undefined symbols for architecture arm64"). BindingLinkerCheck
# typeof()s every bound type, which forces the registrar to reference each one and makes
# that failure happen here instead - once per slice, since the Mac Catalyst framework is a
# separate compile whose exported symbols can differ.
#
# Usage: .github/scripts/build-ios-binding.sh
#
# Environment:
#   SDK9_DIR   directory whose global.json pins the .NET SDK the iOS packages are built
#              with (publish-ios.yml's 9.0.101). The repository root has no global.json,
#              so .NET 10 still runs the file-based scripts; see the iOS toolchain pin in
#              CLAUDE.md for why the build cannot simply use .NET 10.

set -euo pipefail

: "${SDK9_DIR:?SDK9_DIR must point at the directory holding the .NET 9 global.json}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
CONFIGURATION="${BUILD_CONFIGURATION:-Release}"

cd "$SDK9_DIR"
dotnet --version

dotnet build "$REPO_ROOT/src/Maui.RevenueCat.iOS/Maui.RevenueCat.iOS.csproj" -c "$CONFIGURATION"

dotnet publish "$REPO_ROOT/tests/BindingLinkerCheck/BindingLinkerCheck.csproj" \
  -c "$CONFIGURATION" -f net9.0-ios -r ios-arm64

dotnet publish "$REPO_ROOT/tests/BindingLinkerCheck/BindingLinkerCheck.csproj" \
  -c "$CONFIGURATION" -f net9.0-maccatalyst -r maccatalyst-arm64
