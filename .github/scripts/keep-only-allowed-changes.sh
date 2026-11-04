#!/usr/bin/env bash
#
# Reverts every working-tree change that is neither part of the bump itself (the paths
# recorded in <snapshot-file>) nor matched by <allow-regex>. Runs right after the Claude
# repair step, so the deterministic rebuild judges only edits the prompt permitted - a
# "fix" that touched the csproj version, the wrapper or a workflow is discarded here, not
# shipped.
#
# Usage: .github/scripts/keep-only-allowed-changes.sh <snapshot-file> <allow-regex>
#   snapshot-file: `git status --porcelain --untracked-files=all | cut -c4-` taken before
#                  the agent ran (one path per line).
#   allow-regex:   bash extended regex matched against the repo-relative path.

set -euo pipefail

SNAPSHOT="${1:?usage: keep-only-allowed-changes.sh <snapshot-file> <allow-regex>}"
ALLOW="${2:?usage: keep-only-allowed-changes.sh <snapshot-file> <allow-regex>}"

git status --porcelain --untracked-files=all | while IFS= read -r line; do
  status="${line:0:2}"
  path="${line:3}"
  if grep -qxF -- "$path" "$SNAPSHOT"; then
    continue
  fi
  if [[ "$path" =~ $ALLOW ]]; then
    echo "keep    $path"
    continue
  fi
  echo "discard $path (outside the allow-list)"
  if [[ "$status" == "??" ]]; then
    rm -rf -- "$path"
  else
    git checkout -- "$path"
  fi
done
