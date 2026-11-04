#!/usr/bin/env bash
#
# Moves the Android binding onto a newer RevenueCat Android SDK from Maven Central.
#
# Usage: .github/scripts/bump-android.sh <new-version>
#   e.g. .github/scripts/bump-android.sh 11.1.0
#
# What it does:
#   1. Verifies the requested version exists on Maven Central.
#   2. Points <AndroidMavenLibrary> at it - the .NET Android SDK downloads that AAR and its
#      POM when the binding is built, and the POM drives the Java dependency verification
#      that cross-checks the binding's <PackageReference> list.
#   3. Moves every <AndroidIgnoredJavaDependency> onto the version the new POM declares.
#      The reason a dependency is ignored (poko-annotations is source-retention) does not
#      depend on its version, so a version number alone must not fail verification.
#   4. Bumps <Version> (<native>.<binding-rev>: the revision resets to .0 when the native
#      version changes and increments otherwise).
#   5. Compares the AAR's own minSdkVersion with <SupportedOSPlatformVersion> and raises
#      the csproj when the native library requires more.
#   6. Sets <PackageReleaseNotes> to cover every native release since the binding that is
#      live on nuget.org, one line per version, so releases this bump jumps over are still
#      described somewhere. An unreadable upstream source fails the run rather than
#      guessing - the note cannot be corrected once it is published.
#
# It deliberately does NOT touch the wrapper's <PackageReference>: the wrapper is only
# moved onto a binding that is already live on nuget.org, which is a separate step.

set -euo pipefail

NEW_VERSION="${1:?usage: bump-android.sh <new-version>}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
ANDROID_CSPROJ="src/Maui.RevenueCat.Android/Maui.RevenueCat.Android.csproj"
BINDING_PACKAGE_ID="Kebechet.Maui.RevenueCat.Android"
MAVEN_BASE="https://repo1.maven.org/maven2/com/revenuecat/purchases/purchases/${NEW_VERSION}"
POM_URL="${MAVEN_BASE}/purchases-${NEW_VERSION}.pom"
AAR_URL="${MAVEN_BASE}/purchases-${NEW_VERSION}.aar"
CHANGELOG_URL="https://github.com/RevenueCat/purchases-android/releases"

bump() { (cd "$REPO_ROOT" && dotnet run .github/scripts/bump.cs -- "$@"); }

echo "==> Bumping the RevenueCat Android SDK to ${NEW_VERSION}"

# --- 1. Current versions ------------------------------------------------------------
CURRENT_NATIVE=$(bump get-maven-pin "$ANDROID_CSPROJ")
CURRENT_BINDING_VERSION=$(bump get-version "$ANDROID_CSPROJ")

# Empty only when the package has never been published; any other failure exits non-zero
# and set -e stops here, because falling back to the csproj would quietly anchor the note
# to a version no consumer ever had.
LAST_PUBLISHED_NATIVE=$(bump last-published-native "$BINDING_PACKAGE_ID")

echo "    current native version:  $CURRENT_NATIVE"
echo "    current binding version: $CURRENT_BINDING_VERSION"
echo "    last published native:   ${LAST_PUBLISHED_NATIVE:-<nothing published yet>}"
echo "    target  native version:  $NEW_VERSION"

# --- 2. The target must exist on Maven Central ----------------------------------------
# The AAR is only fetched at build time, so a typo here would otherwise surface as an
# opaque XA4234 much later. --retry (without --retry-all-errors) covers timeouts and 5xx
# but not the 404 being tested for.
WORK_TMP="$(mktemp -d)"
trap 'rm -rf "$WORK_TMP"' EXIT
echo "==> Downloading $POM_URL"
if ! curl -fsSL --retry 3 --retry-delay 2 -o "$WORK_TMP/purchases.pom" "$POM_URL"; then
  echo "ERROR: com.revenuecat.purchases:purchases:${NEW_VERSION} not found on Maven Central" >&2
  exit 1
fi

# --- 3. New binding version -------------------------------------------------------------
if [[ "$CURRENT_NATIVE" == "$NEW_VERSION" ]]; then
  NEW_BINDING_VERSION="${CURRENT_BINDING_VERSION%.*}.$(( ${CURRENT_BINDING_VERSION##*.} + 1 ))"
else
  NEW_BINDING_VERSION="${NEW_VERSION}.0"
fi
echo "    new     binding version: $NEW_BINDING_VERSION"

# --- 4. Minimum Android API the AAR itself requires ---------------------------------------
# An AAR's manifest is plain XML, so this needs no aapt. A native SDK that raises its
# minSdk while the binding still claims a lower one compiles and packs cleanly and only
# breaks at deployment.
if ! curl -fsSL --retry 3 --retry-delay 2 -o "$WORK_TMP/purchases.aar" "$AAR_URL"; then
  echo "ERROR: could not download $AAR_URL to read its minSdkVersion" >&2
  exit 1
fi
NATIVE_MIN_OS=$(unzip -p "$WORK_TMP/purchases.aar" AndroidManifest.xml 2>/dev/null \
  | sed -n -E 's|.*android:minSdkVersion="([^"]+)".*|\1|p' | head -1)
if [[ -z "$NATIVE_MIN_OS" ]]; then
  echo "ERROR: could not read android:minSdkVersion from the AAR manifest" >&2
  exit 1
fi

echo "==> Native AAR requires API ${NATIVE_MIN_OS}"
MIN_OS_OUTPUT=$(bump check-min-os "$ANDROID_CSPROJ" "$NATIVE_MIN_OS")
MIN_OS_RAISED=$(printf '%s\n' "$MIN_OS_OUTPUT" | sed -n -E 's|^min_os_raised=(.*)$|\1|p')
MIN_OS_PREVIOUS=$(printf '%s\n' "$MIN_OS_OUTPUT" | sed -n -E 's|^min_os_previous=(.*)$|\1|p')

# --- 5. Ignored Java dependencies follow the POM ---------------------------------------
bump sync-ignored-java-dependencies "$ANDROID_CSPROJ" "$WORK_TMP/purchases.pom"

# --- 6. Release note --------------------------------------------------------------------
# Anchored to the newest binding on nuget.org rather than to the csproj: the two are the
# same only while every bump gets published. A version merged and superseded before it was
# published has no package of its own, so its upstream notes appear here or nowhere.
if [[ -n "$LAST_PUBLISHED_NATIVE" ]]; then
  NOTE_FROM="$LAST_PUBLISHED_NATIVE"
else
  NOTE_FROM="$CURRENT_NATIVE"
fi

if [[ "$NOTE_FROM" == "$NEW_VERSION" ]]; then
  EXCERPT=""
  NOTE="${NEW_BINDING_VERSION}: rebuilt the binding for RevenueCat Android SDK ${NEW_VERSION} (binding revision only, no native change)."
else
  EXCERPT=$(bump changelog-range android "$NOTE_FROM" "$NEW_VERSION")
  NOTE="${NEW_BINDING_VERSION}: RevenueCat Android SDK updated from ${NOTE_FROM} to ${NEW_VERSION}. RevenueCat SDK changes per version (RevenueCatUI is not bound):"
fi
if [[ -n "$EXCERPT" ]]; then
  NOTE+=$'\n'"${EXCERPT}"
fi
if [[ "$MIN_OS_RAISED" == "true" ]]; then
  NOTE+=$'\n'"BREAKING: the minimum supported Android API level is now ${NATIVE_MIN_OS} (was ${MIN_OS_PREVIOUS}), as required by the native SDK."
fi
NOTE+=$'\n'"Full changelog: ${CHANGELOG_URL}"
echo "    release note:"
printf '%s\n' "$NOTE" | sed 's/^/      /'

# A new major is breaking by RevenueCat's own semver even when no release in the range has
# a "Breaking Changes" section, so either signal keeps the PR a draft.
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

# --- 7. Edit the csproj -----------------------------------------------------------------
bump set-maven-pin "$ANDROID_CSPROJ" "$NEW_VERSION"
bump set-version "$ANDROID_CSPROJ" "$NEW_BINDING_VERSION"
bump set-release-note "$ANDROID_CSPROJ" "$NOTE"

# --- 8. Read everything back: a silently missed edit would ship "11.1.0.0" containing
#        RevenueCat 10.16.1, which is worse than failing here. -----------------------------
WRITTEN_NATIVE=$(bump get-maven-pin "$ANDROID_CSPROJ")
if [[ "$WRITTEN_NATIVE" != "$NEW_VERSION" ]]; then
  echo "ERROR: failed to update the <AndroidMavenLibrary> pin (still '${WRITTEN_NATIVE}')" >&2
  exit 1
fi
WRITTEN_BINDING=$(bump get-version "$ANDROID_CSPROJ")
if [[ "$WRITTEN_BINDING" != "$NEW_BINDING_VERSION" ]]; then
  echo "ERROR: failed to update <Version> (still '${WRITTEN_BINDING}')" >&2
  exit 1
fi
WRITTEN_NOTE=$(bump get-release-note "$ANDROID_CSPROJ" | tr -d '\r')
if [[ "$WRITTEN_NOTE" != "$NOTE" ]]; then
  echo "ERROR: failed to write the <PackageReleaseNotes> entry" >&2
  exit 1
fi

echo "==> Done"
echo "    binding version: $NEW_BINDING_VERSION"
echo "    files changed:"
echo "      - $ANDROID_CSPROJ"

if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  # The note and the excerpt are multi-line, and a bare `name=value` carrying newlines
  # makes the runner reject the whole output file - so those two go in heredoc form.
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
