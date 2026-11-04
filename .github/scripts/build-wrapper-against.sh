#!/usr/bin/env bash
#
# Compiles the cross-platform wrapper against a binding that is NOT on nuget.org yet: the
# freshly packed nupkg in <feed-dir>. This is the consumer-side check a bump PR must pass
# before it is called green - a native SDK that silently drops or renames an API the
# wrapper uses would otherwise only surface when the wrapper is moved onto the binding.
#
# Usage: .github/scripts/build-wrapper-against.sh <android|ios> <feed-dir> <binding-version>
#
# Prints the path of the built wrapper assembly as its last line, for the API report.
#
# Environment:
#   DOTNET_BUILD_DIR  optional directory to run restore/build from, so a global.json there
#                     picks the SDK (the iOS job pins 9.0.101 that way and keeps .NET 10
#                     for the file-based bump.cs)
#
# The wrapper's <PackageReference> version is rewritten in place for the duration of the
# build and restored from a byte copy on exit - including when the build fails, when the
# script is interrupted, and when a command under `set -e` aborts it. The bump itself never
# carries that edit: open-bump-pr.sh refuses to commit unless this file is byte-identical
# to HEAD.
#
# The feed is wired through a throwaway nuget.config rather than `--source`: when NuGet
# decides any --source value is relative it resolves ALL of them against the project
# directory, and the nuget.org URL turns into a non-existent local path (NU1301).

set -euo pipefail

PLATFORM="${1:?usage: build-wrapper-against.sh <android|ios> <feed-dir> <binding-version>}"
FEED="${2:?usage: build-wrapper-against.sh <android|ios> <feed-dir> <binding-version>}"
VERSION="${3:?usage: build-wrapper-against.sh <android|ios> <feed-dir> <binding-version>}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
WRAPPER="$REPO_ROOT/src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj"
CONFIGURATION="${BUILD_CONFIGURATION:-Release}"
BUILD_DIR="${DOTNET_BUILD_DIR:-$REPO_ROOT}"

case "$PLATFORM" in
  android) TFM="net9.0-android"; PACKAGE_ID="Kebechet.Maui.RevenueCat.Android" ;;
  ios)     TFM="net9.0-ios";     PACKAGE_ID="Kebechet.Maui.RevenueCat.iOS" ;;
  *) echo "ERROR: unknown platform '$PLATFORM' (expected android or ios)" >&2; exit 2 ;;
esac

if ! ls "$FEED"/*.nupkg >/dev/null 2>&1; then
  echo "ERROR: no .nupkg in feed directory '$FEED'" >&2
  exit 1
fi

FEED_ABS=$(cd "$FEED" && pwd)
WRAPPER_PATH="$WRAPPER"
# Git Bash on Windows: dotnet needs Windows paths, not /e/... (only matters locally).
if command -v cygpath >/dev/null 2>&1; then
  FEED_ABS=$(cygpath -w "$FEED_ABS")
  WRAPPER_PATH=$(cygpath -w "$WRAPPER")
fi

CONFIG="$FEED/nuget.config"
cat > "$CONFIG" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="bump-feed" value="$FEED_ABS" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF
CONFIG_PATH="$CONFIG"
if command -v cygpath >/dev/null 2>&1; then
  CONFIG_PATH=$(cygpath -w "$CONFIG")
fi

# --- Point the wrapper at the unpublished version, and put it back no matter what --------
BACKUP=$(mktemp)
cp "$WRAPPER" "$BACKUP"
restore_wrapper() {
  local status=$?
  cp "$BACKUP" "$WRAPPER"
  rm -f "$BACKUP"
  return $status
}
trap restore_wrapper EXIT

(cd "$REPO_ROOT" && dotnet run .github/scripts/bump.cs -- set-package-version "$WRAPPER" "$PACKAGE_ID" "$VERSION")

if ! grep -qF "\"$PACKAGE_ID\" Version=\"$VERSION\"" "$WRAPPER"; then
  echo "ERROR: could not point $PACKAGE_ID at $VERSION in $WRAPPER" >&2
  grep -n "$PACKAGE_ID" "$WRAPPER" >&2 || true
  exit 1
fi
echo "==> the wrapper temporarily references $PACKAGE_ID $VERSION"

echo "==> Restoring the wrapper ($TFM) from $FEED_ABS"
(cd "$BUILD_DIR" && dotnet restore "$WRAPPER_PATH" --configfile "$CONFIG_PATH" -p:TargetFrameworks="$TFM")

echo "==> Building the wrapper ($TFM)"
(cd "$BUILD_DIR" && dotnet build "$WRAPPER_PATH" --no-restore -c "$CONFIGURATION" \
  -p:TargetFrameworks="$TFM" -p:GeneratePackageOnBuild=false)

echo "==> the wrapper compiles against $PACKAGE_ID $VERSION"
# Looked up, not spelled out: on macOS the iOS SDK puts a platform segment into the output
# path (bin/ios/Release/net9.0-ios).
find "$REPO_ROOT/src/Maui.RevenueCat.InAppBilling/bin" -path "*/$CONFIGURATION/$TFM/Maui.RevenueCat.dll" | head -1
