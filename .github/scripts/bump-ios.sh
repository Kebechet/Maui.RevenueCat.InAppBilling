#!/usr/bin/env bash
#
# Moves the iOS binding onto a newer RevenueCat iOS SDK, from the outputs the TryBumpIOS
# workflow has already produced: an xcframework built from source with
# BYPASS_SIMULATED_STORE_RELEASE_CHECK (issue #116) and post-processed Sharpie output.
#
# Usage: .github/scripts/bump-ios.sh <new-version> <generated-dir> <xcframework>
#   generated-dir: holds the post-processed ApiDefinitions.cs and StructsAndEnums.cs
#   xcframework:   the cleaned RevenueCat.xcframework (no .swiftmodule / .dSYM)
#
# What it does:
#   1. Copies the bindings and the xcframework into src/Maui.RevenueCat.iOS/. The copy
#      keeps symlinks: the Mac Catalyst slice is a macOS framework bundle (Versions/A plus
#      top-level symlinks), and a dereferenced copy builds fine but ships a broken layout.
#   2. Bumps <Version> (<native>.<binding-rev>).
#   3. Compares the framework's MinimumOSVersion with <SupportedOSPlatformVersion> and
#      raises the csproj when the native SDK requires more.
#   4. Sets <PackageReleaseNotes> to cover every native release since the binding that is
#      live on nuget.org, one line per version.
#
# macOS only: plutil reads the framework Info.plist, which xcodebuild writes in binary form.

set -euo pipefail

NEW_VERSION="${1:?usage: bump-ios.sh <new-version> <generated-dir> <xcframework>}"
GENERATED_DIR="${2:?usage: bump-ios.sh <new-version> <generated-dir> <xcframework>}"
XCFRAMEWORK="${3:?usage: bump-ios.sh <new-version> <generated-dir> <xcframework>}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
BINDING_DIR="src/Maui.RevenueCat.iOS"
IOS_CSPROJ="${BINDING_DIR}/Maui.RevenueCat.iOS.csproj"
BINDING_PACKAGE_ID="Kebechet.Maui.RevenueCat.iOS"
CHANGELOG_URL="https://github.com/RevenueCat/purchases-ios/releases"

bump() { (cd "$REPO_ROOT" && dotnet run .github/scripts/bump.cs -- "$@"); }

echo "==> Bumping the RevenueCat iOS SDK to ${NEW_VERSION}"

for f in "$GENERATED_DIR/ApiDefinitions.cs" "$GENERATED_DIR/StructsAndEnums.cs" "$XCFRAMEWORK/Info.plist"; do
  if [[ ! -f "$f" ]]; then
    echo "ERROR: $f not found - the generation steps did not produce it" >&2
    exit 1
  fi
done

# --- 1. Current versions ------------------------------------------------------------
CURRENT_BINDING_VERSION=$(bump get-version "$IOS_CSPROJ")
CURRENT_NATIVE="${CURRENT_BINDING_VERSION%.*}"
LAST_PUBLISHED_NATIVE=$(bump last-published-native "$BINDING_PACKAGE_ID")

echo "    current native version:  $CURRENT_NATIVE"
echo "    current binding version: $CURRENT_BINDING_VERSION"
echo "    last published native:   ${LAST_PUBLISHED_NATIVE:-<nothing published yet>}"
echo "    target  native version:  $NEW_VERSION"

# --- 2. New binding version -------------------------------------------------------------
if [[ "$CURRENT_NATIVE" == "$NEW_VERSION" ]]; then
  NEW_BINDING_VERSION="${CURRENT_BINDING_VERSION%.*}.$(( ${CURRENT_BINDING_VERSION##*.} + 1 ))"
else
  NEW_BINDING_VERSION="${NEW_VERSION}.0"
fi
echo "    new     binding version: $NEW_BINDING_VERSION"

# --- 3. Bindings and framework into place ------------------------------------------------
cp "$GENERATED_DIR/ApiDefinitions.cs" "$REPO_ROOT/$BINDING_DIR/ApiDefinitions.cs"
cp "$GENERATED_DIR/StructsAndEnums.cs" "$REPO_ROOT/$BINDING_DIR/StructsAndEnums.cs"
rm -rf "$REPO_ROOT/$BINDING_DIR/nativelib/RevenueCat.xcframework"
cp -a "$XCFRAMEWORK" "$REPO_ROOT/$BINDING_DIR/nativelib/RevenueCat.xcframework"

CATALYST_FRAMEWORK="$REPO_ROOT/$BINDING_DIR/nativelib/RevenueCat.xcframework/ios-arm64_x86_64-maccatalyst/RevenueCat.framework"
for link in Headers Modules Resources RevenueCat Versions/Current; do
  if [[ ! -L "$CATALYST_FRAMEWORK/$link" ]]; then
    echo "ERROR: $CATALYST_FRAMEWORK/$link is not a symlink - the Mac Catalyst bundle layout was flattened" >&2
    exit 1
  fi
done

# --- 4. Minimum iOS the framework itself requires --------------------------------------
FRAMEWORK_PLIST="$REPO_ROOT/$BINDING_DIR/nativelib/RevenueCat.xcframework/ios-arm64/RevenueCat.framework/Info.plist"
NATIVE_MIN_OS=$(plutil -extract MinimumOSVersion raw -o - "$FRAMEWORK_PLIST")
if [[ -z "$NATIVE_MIN_OS" ]]; then
  echo "ERROR: could not read MinimumOSVersion from $FRAMEWORK_PLIST" >&2
  exit 1
fi

echo "==> Native framework requires iOS ${NATIVE_MIN_OS}"
MIN_OS_OUTPUT=$(bump check-min-os "$IOS_CSPROJ" "$NATIVE_MIN_OS")
MIN_OS_RAISED=$(printf '%s\n' "$MIN_OS_OUTPUT" | sed -n -E 's|^min_os_raised=(.*)$|\1|p')
MIN_OS_PREVIOUS=$(printf '%s\n' "$MIN_OS_OUTPUT" | sed -n -E 's|^min_os_previous=(.*)$|\1|p')

# --- 5. Release note --------------------------------------------------------------------
if [[ -n "$LAST_PUBLISHED_NATIVE" ]]; then
  NOTE_FROM="$LAST_PUBLISHED_NATIVE"
else
  NOTE_FROM="$CURRENT_NATIVE"
fi

if [[ "$NOTE_FROM" == "$NEW_VERSION" ]]; then
  EXCERPT=""
  NOTE="${NEW_BINDING_VERSION}: rebuilt the binding for RevenueCat iOS SDK ${NEW_VERSION} (binding revision only, no native change)."
else
  EXCERPT=$(bump changelog-range ios "$NOTE_FROM" "$NEW_VERSION")
  NOTE="${NEW_BINDING_VERSION}: RevenueCat iOS SDK updated from ${NOTE_FROM} to ${NEW_VERSION}. The xcframework is built from source with BYPASS_SIMULATED_STORE_RELEASE_CHECK, so Test Store keys work in Release. RevenueCat SDK changes per version (RevenueCatUI is not bound):"
fi
if [[ -n "$EXCERPT" ]]; then
  NOTE+=$'\n'"${EXCERPT}"
fi
if [[ "$MIN_OS_RAISED" == "true" ]]; then
  NOTE+=$'\n'"BREAKING: the minimum supported iOS version is now ${NATIVE_MIN_OS} (was ${MIN_OS_PREVIOUS}), as required by the native SDK."
fi
NOTE+=$'\n'"Full changelog: ${CHANGELOG_URL}"
echo "    release note:"
printf '%s\n' "$NOTE" | sed 's/^/      /'

UPSTREAM_BREAKING=false
if printf '%s' "$EXCERPT" | grep -q 'BREAKING:'; then
  UPSTREAM_BREAKING=true
fi
if [[ "${NOTE_FROM%%.*}" != "${NEW_VERSION%%.*}" ]]; then
  UPSTREAM_BREAKING=true
fi
if [[ "$UPSTREAM_BREAKING" == "true" ]]; then
  echo "==> upstream marked a breaking change in this range"
fi

# --- 6. Edit the csproj and read it back -------------------------------------------------
bump set-version "$IOS_CSPROJ" "$NEW_BINDING_VERSION"
bump set-release-note "$IOS_CSPROJ" "$NOTE"

WRITTEN_BINDING=$(bump get-version "$IOS_CSPROJ")
if [[ "$WRITTEN_BINDING" != "$NEW_BINDING_VERSION" ]]; then
  echo "ERROR: failed to update <Version> (still '${WRITTEN_BINDING}')" >&2
  exit 1
fi
WRITTEN_NOTE=$(bump get-release-note "$IOS_CSPROJ" | tr -d '\r')
if [[ "$WRITTEN_NOTE" != "$NOTE" ]]; then
  echo "ERROR: failed to write the <PackageReleaseNotes> entry" >&2
  exit 1
fi

echo "==> Done"
echo "    binding version: $NEW_BINDING_VERSION"

if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  {
    echo "native_version=${NEW_VERSION}"
    echo "binding_version=${NEW_BINDING_VERSION}"
    echo "previous_native_version=${CURRENT_NATIVE}"
    echo "note_from=${NOTE_FROM}"
    echo "upstream_breaking=${UPSTREAM_BREAKING}"
    echo "changelog_excerpt<<RC_BUMP_EOF"
    printf '%s\n' "$EXCERPT"
    echo "RC_BUMP_EOF"
    echo "release_note<<RC_BUMP_EOF"
    printf '%s\n' "$NOTE"
    echo "RC_BUMP_EOF"
  } >> "$GITHUB_OUTPUT"
fi
