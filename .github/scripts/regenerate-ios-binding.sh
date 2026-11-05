#!/usr/bin/env bash
#
# Re-runs the post-processor over the raw Sharpie output of this run and copies the result
# into src/Maui.RevenueCat.iOS/. The repair step uses it after extending the post-processor
# (a new LinkerRiskSymbols entry, a new pass), so the committed ApiDefinitions.cs is what
# the committed post-processor produces - and the next bump reproduces the fix by itself
# instead of hitting the same failure again.
#
# Usage: .github/scripts/regenerate-ios-binding.sh
#
# Environment:
#   SHARPIE_RAW_DIR  the untouched `sharpie bind` output (ApiDefinitions.cs, StructsAndEnums.cs)

set -euo pipefail

: "${SHARPIE_RAW_DIR:?SHARPIE_RAW_DIR must point at the raw sharpie output}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

cp "$SHARPIE_RAW_DIR/ApiDefinitions.cs" "$SHARPIE_RAW_DIR/StructsAndEnums.cs" "$WORK/"
# The bump has already overwritten the working copy, so the published binding is the
# committed one.
git -C "$REPO_ROOT" show HEAD:src/Maui.RevenueCat.iOS/ApiDefinitions.cs > "$WORK/published-ApiDefinitions.cs"
(cd "$REPO_ROOT" && dotnet run .github/scripts/postprocess_ios_bindings.cs -- \
  "$WORK/ApiDefinitions.cs" "$WORK/StructsAndEnums.cs" --report "$WORK/manual_review.md" \
  --published "$WORK/published-ApiDefinitions.cs")

cp "$WORK/ApiDefinitions.cs" "$REPO_ROOT/src/Maui.RevenueCat.iOS/ApiDefinitions.cs"
cp "$WORK/StructsAndEnums.cs" "$REPO_ROOT/src/Maui.RevenueCat.iOS/StructsAndEnums.cs"
echo "==> regenerated src/Maui.RevenueCat.iOS/ApiDefinitions.cs and StructsAndEnums.cs"
