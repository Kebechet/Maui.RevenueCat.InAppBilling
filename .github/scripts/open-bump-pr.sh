#!/usr/bin/env bash
#
# Commits the binding bump, pushes bump/<platform>-<target> and creates or refreshes its
# pull request against BASE_BRANCH. One script for both platforms so the PR contract lives
# in one place:
#   - only COMMIT_PATHS are committed; the wrapper csproj must be byte-identical
#   - STATUS=green           -> ready-for-review PR
#   - STATUS=<anything else> -> DRAFT PR labelled with that status
#
# Inputs (environment):
#   PLATFORM          android | ios
#   PREVIOUS, TARGET  native SDK versions
#   BINDING_VERSION   <native>.<binding-rev>
#   BASE_BRANCH       branch the PR targets (main on the schedule; the dispatched ref otherwise)
#   STATUS            green | binding-broken | wrapper-broken | min-os-raised | api-rebound | upstream-breaking
#   COMMIT_PATHS      optional, space-separated pathspecs to commit (default: the binding dir).
#                     iOS adds the post-processor and its tests, which a repair may extend.
#   MIN_OS_RAISED, MIN_OS_PREVIOUS, MIN_OS_CURRENT
#                     from bump.cs check-min-os; a raised floor is breaking for consumers,
#                     so that PR opens as a draft even though it builds
#   CHANGELOG_EXCERPT optional upstream changelog, one line per version in the range
#   NOTE_FROM         native version the release note is anchored to: the newest binding on
#                     nuget.org, which is not PREVIOUS once a bump merges unpublished
#   API_REPORT_FILE   optional markdown from bump.cs api-report
#   CLAUDE_FIXED      true when the Claude repair step changed sources that then built
#   REPAIR_ATTEMPTED  true when the Claude repair step ran, whatever it achieved
#   FIX_SUMMARY_FILE  optional markdown written by the repair step
#   RUN_URL           link to the workflow run (build logs are attached there)
#   GH_TOKEN          token for gh (the bump-bot app token, so pushes raise events)
#   DRY_RUN           1 = commit locally but print instead of pushing / calling gh
#
# Outputs (GITHUB_OUTPUT): pr_number, pr_url, branch

set -euo pipefail

: "${PLATFORM:?}" "${PREVIOUS:?}" "${TARGET:?}" "${BINDING_VERSION:?}" "${BASE_BRANCH:?}" "${STATUS:?}"
CHANGELOG_EXCERPT="${CHANGELOG_EXCERPT:-}"
NOTE_FROM="${NOTE_FROM:-}"
MIN_OS_RAISED="${MIN_OS_RAISED:-false}"
MIN_OS_PREVIOUS="${MIN_OS_PREVIOUS:-}"
MIN_OS_CURRENT="${MIN_OS_CURRENT:-}"
WRAPPER_OK="${WRAPPER_OK:-true}"
API_REPORT_FILE="${API_REPORT_FILE:-}"
CLAUDE_FIXED="${CLAUDE_FIXED:-false}"
REPAIR_ATTEMPTED="${REPAIR_ATTEMPTED:-false}"
FIX_SUMMARY_FILE="${FIX_SUMMARY_FILE:-}"
RUN_URL="${RUN_URL:-}"
DRY_RUN="${DRY_RUN:-0}"

WRAPPER_CSPROJ="src/Maui.RevenueCat.InAppBilling/Maui.RevenueCat.csproj"
case "$PLATFORM" in
  android)
    PLATFORM_NAME="Android"; WORKFLOW="TryBumpAndroid"; OS_NAME="Android API level"
    BINDING_DIR="src/Maui.RevenueCat.Android"; TFM="net9.0-android"; SCOPE="android-binding"
    SOURCE_URL="https://central.sonatype.com/artifact/com.revenuecat.purchases/purchases/${TARGET}"
    CHANGELOG_URL="https://github.com/RevenueCat/purchases-android/releases" ;;
  ios)
    PLATFORM_NAME="iOS"; WORKFLOW="TryBumpIOS"; OS_NAME="iOS"
    BINDING_DIR="src/Maui.RevenueCat.iOS"; TFM="net9.0-ios"; SCOPE="ios-binding"
    SOURCE_URL="https://github.com/RevenueCat/purchases-ios/releases/tag/${TARGET}"
    CHANGELOG_URL="https://github.com/RevenueCat/purchases-ios/releases" ;;
  *) echo "ERROR: unknown platform '$PLATFORM' (expected android or ios)" >&2; exit 2 ;;
esac
read -r -a PATHS <<< "${COMMIT_PATHS:-$BINDING_DIR}"

BRANCH="bump/${PLATFORM}-${TARGET}"
TITLE="build(${SCOPE}): bump RevenueCat ${PLATFORM_NAME} SDK to ${TARGET}"
# NuGet drops a trailing ".0" from a four-part version, so say what nuget.org will show.
PUBLISHED_VERSION=$(printf '%s' "$BINDING_VERSION" | sed -E 's/\.0$//')

# --- 1. Guard: the wrapper is never part of a bump ------------------------------------
if ! git diff --quiet -- "$WRAPPER_CSPROJ"; then
  echo "ERROR: $WRAPPER_CSPROJ was modified; a bump PR must leave the wrapper untouched" >&2
  git --no-pager diff -- "$WRAPPER_CSPROJ" >&2
  exit 1
fi

# --- 2. Commit only the bump paths ----------------------------------------------------------
COMMIT_BODY="From ${PREVIOUS} to ${TARGET}. Binding only - the wrapper moves onto ${BINDING_VERSION} once it is live on nuget.org."
if [[ -n "$CHANGELOG_EXCERPT" ]]; then
  COMMIT_BODY+=$'\n\n'"Upstream, since the binding that is live on nuget.org:"$'\n'"${CHANGELOG_EXCERPT}"
fi
if [[ "$MIN_OS_RAISED" == "true" ]]; then
  COMMIT_BODY+=$'\n\n'"BREAKING: the native SDK raised its floor, so the binding's SupportedOSPlatformVersion moves from ${MIN_OS_PREVIOUS} to ${MIN_OS_CURRENT}."
fi
if [[ "$CLAUDE_FIXED" == "true" ]]; then
  COMMIT_BODY+=$'\n\n'"Binding sources were repaired by Claude Code so they compile against ${TARGET}; see the pull request description."
fi
if [[ "$STATUS" != "green" ]]; then
  COMMIT_BODY+=$'\n\n'"Status: ${STATUS} - opened as a draft, not mergeable as-is."
fi

# A dry run is for trying the pipeline on a working copy, so it neither switches branches
# nor commits there.
if [[ "$DRY_RUN" == "1" ]]; then
  echo "[dry-run] would commit on $BRANCH:"
  printf '%s\n\n%s\n\n' "$TITLE" "$COMMIT_BODY"
  git status --short -- "${PATHS[@]}"
else
  git config user.name  "kebechet-bump-bot[bot]"
  git config user.email "kebechet-bump-bot[bot]@users.noreply.github.com"
  git checkout -q -B "$BRANCH"
  git add -A -- "${PATHS[@]}"
  if git diff --cached --quiet; then
    echo "ERROR: nothing to commit under ${PATHS[*]}" >&2
    exit 1
  fi
  git commit -q -m "$TITLE" -m "$COMMIT_BODY"
  echo "==> committed $(git rev-parse --short HEAD) on $BRANCH"
  git --no-pager show --stat --format='%s%n%n%b' HEAD | cat
fi

# The Mac Catalyst slice is a macOS framework bundle whose top-level entries are symlinks.
# A copy or checkout that dereferenced them still builds, so the commit is the place to
# catch it: every one of them has to land as a mode-120000 entry.
if [[ "$PLATFORM" == "ios" && "$DRY_RUN" != "1" ]]; then
  CATALYST="$BINDING_DIR/nativelib/RevenueCat.xcframework/ios-arm64_x86_64-maccatalyst/RevenueCat.framework"
  for link in Headers Modules Resources RevenueCat Versions/Current; do
    mode=$(git ls-tree HEAD -- "$CATALYST/$link" | awk '{print $1}')
    if [[ "$mode" != "120000" ]]; then
      echo "ERROR: $CATALYST/$link is committed with mode '${mode:-missing}', not as a symlink" >&2
      exit 1
    fi
  done
fi

# --- 3. PR body -------------------------------------------------------------------------------
if [[ "$STATUS" == "green" ]]; then
  STATUS_LINE="✅ **Ready to merge.** The binding builds, the wrapper compiles against it, nothing else changed."
  BUILD_LINE="✅ Binding builds (\`dotnet build -c Release\`)"
  WRAPPER_LINE="✅ Wrapper compiles against the packed binding (\`${TFM}\`, wrapper csproj untouched)"
elif [[ "$STATUS" == "binding-broken" ]]; then
  STATUS_LINE="🛑 **DRAFT - the binding does not build.** Do not merge. Fix it on this branch (or wait for the next run to retry) and mark the PR ready."
  BUILD_LINE="❌ Binding does NOT build - see the \`binding-build-log-${PLATFORM}\` artifact on the run"
  WRAPPER_LINE="⏭️ Wrapper check skipped because the binding is broken"
elif [[ "$STATUS" == "min-os-raised" ]]; then
  STATUS_LINE="🛑 **DRAFT - this bump raises the minimum ${OS_NAME} to \`${MIN_OS_CURRENT}\` (was \`${MIN_OS_PREVIOUS}\`).** The binding builds, but raising the floor is a breaking change for consumers, so it is a human decision rather than an automatic merge. Decide whether the wrapper's own \`SupportedOSPlatformVersion\` moves with it, then mark the PR ready."
  BUILD_LINE="✅ Binding builds (\`dotnet build -c Release\`)"
  if [[ "$WRAPPER_OK" == "true" ]]; then
    WRAPPER_LINE="✅ Wrapper compiles against the packed binding (\`${TFM}\`, wrapper csproj untouched)"
  else
    # Expected: NuGet will not hand a higher-platform asset to a project whose own floor
    # is lower, so this fails until the wrapper's floor moves too.
    WRAPPER_LINE="❌ Wrapper does NOT compile against it - expected while the wrapper's own floor is below \`${MIN_OS_CURRENT}\`; see the \`wrapper-build-log-${PLATFORM}\` artifact"
  fi
elif [[ "$STATUS" == "api-rebound" ]]; then
  STATUS_LINE="🛑 **DRAFT - binding members keep their C# signature but now call a different native method** (the 🛑 list under Native API changes). Code written against the published binding still compiles and silently does something else. Make each one bind what it bound before (the iOS post-processor's \`--published\` rule, or a new name for the new native member), or accept the change on purpose, then mark the PR ready."
  BUILD_LINE="✅ Binding builds (\`dotnet build -c Release\`)"
  WRAPPER_LINE="✅ Wrapper compiles against the packed binding (\`${TFM}\`, wrapper csproj untouched)"
elif [[ "$STATUS" == "upstream-breaking" ]]; then
  STATUS_LINE="🛑 **DRAFT - RevenueCat marked a breaking change in this range (or it is a new major).** Everything builds and the binding's own minimum OS version is unchanged, but read the \`BREAKING:\` and \`WARNING:\` lines above before merging: the break can be a build requirement for consumers (Android 11 requires compiling against API 35) rather than anything visible in the API. Mark the PR ready once you have decided it is safe for consumers."
  BUILD_LINE="✅ Binding builds (\`dotnet build -c Release\`)"
  WRAPPER_LINE="✅ Wrapper compiles against the packed binding (\`${TFM}\`, wrapper csproj untouched)"
else
  STATUS_LINE="🛑 **DRAFT - the wrapper does not compile against this binding.** The native API changed in a way the wrapper depends on; that is a wrapper change for a human, not something to merge as-is."
  BUILD_LINE="✅ Binding builds"
  WRAPPER_LINE="❌ Wrapper does NOT compile against it (\`${TFM}\`) - see the \`wrapper-build-log-${PLATFORM}\` artifact on the run"
fi

if [[ -n "$CHANGELOG_EXCERPT" ]]; then
  EXCERPT_BLOCK=$(printf '%s\n' "$CHANGELOG_EXCERPT" | sed 's/^/- /')
else
  EXCERPT_BLOCK="_No upstream release in range - binding revision only._"
fi

# The notes cover every release since what is actually on nuget.org, which is not the
# repo's previous pin whenever a bump merged without being published. Both rows are shown
# so a reviewer can see the gap rather than infer it.
if [[ -n "$NOTE_FROM" && "$NOTE_FROM" != "$TARGET" ]]; then
  EXCERPT_HEADING="\`${NOTE_FROM}\` → \`${TARGET}\`"
  PUBLISHED_FROM_ROW="\`${NOTE_FROM}\`"
  if [[ "$NOTE_FROM" != "$PREVIOUS" ]]; then
    PUBLISHED_FROM_ROW+=" ⚠️ the repo is ahead of nuget.org, so this bump also carries every release in between"
  fi
else
  EXCERPT_HEADING="${TARGET}"
  PUBLISHED_FROM_ROW="_nothing published yet_"
fi

if [[ "$MIN_OS_RAISED" == "true" ]]; then
  MIN_OS_ROW="⚠️ raised \`${MIN_OS_PREVIOUS}\` → \`${MIN_OS_CURRENT}\` (required by the native SDK)"
elif [[ -n "$MIN_OS_CURRENT" ]]; then
  MIN_OS_ROW="\`${MIN_OS_CURRENT}\` (unchanged)"
else
  MIN_OS_ROW="unchanged"
fi

if [[ -n "$API_REPORT_FILE" && -s "$API_REPORT_FILE" ]]; then
  API_BLOCK="$(cat "$API_REPORT_FILE")"
elif [[ "$STATUS" == "binding-broken" ]]; then
  API_BLOCK="_Not produced - the binding did not build, so there is no new surface to compare._"
else
  API_BLOCK="⚠️ _Not produced - the report step failed; see its log on the run._"
fi

FIX_SUMMARY="_(the repair step wrote no summary)_"
if [[ -n "$FIX_SUMMARY_FILE" && -s "$FIX_SUMMARY_FILE" ]]; then
  FIX_SUMMARY="$(cat "$FIX_SUMMARY_FILE")"
fi
if [[ "$CLAUDE_FIXED" == "true" ]]; then
  FIX_BLOCK="🤖 **Claude Code repaired the binding sources** so they compile against ${TARGET}. Review those edits with the same care as a hand-written change."$'\n\n'"${FIX_SUMMARY}"
elif [[ "$STATUS" == "binding-broken" && "$REPAIR_ATTEMPTED" == "true" ]]; then
  # The repair writes its summary whether or not it succeeded, and a failed attempt is
  # where a reviewer needs it most: it says what was tried and why it stopped.
  FIX_BLOCK="🛑 **The Claude Code repair ran, and the binding still does not build.** Its own account follows; the full transcript is the \`claude-repair-transcript-${PLATFORM}\` artifact on the run (kept 7 days)."$'\n\n'"${FIX_SUMMARY}"
elif [[ "$STATUS" == "binding-broken" ]]; then
  FIX_BLOCK="⚠️ No repair was attempted: \`CLAUDE_CODE_OAUTH_TOKEN\` is not configured."
else
  FIX_BLOCK="No binding source changes were needed beyond the bump itself."
fi

RUN_LINE=""
if [[ -n "$RUN_URL" ]]; then
  RUN_LINE="Workflow run (build logs and the full API report are attached as artifacts): ${RUN_URL}"
fi

BODY=$(cat <<EOF
Binding-only bump produced by \`${WORKFLOW}\`. The wrapper is moved onto \`${BINDING_VERSION}\` in a separate step once it is live on nuget.org.

| | |
|---|---|
| Previous (in repo) | \`${PREVIOUS}\` |
| Previous (on nuget.org) | ${PUBLISHED_FROM_ROW} |
| Target | \`${TARGET}\` |
| Binding version | \`${BINDING_VERSION}\` (nuget.org shows it as \`${PUBLISHED_VERSION}\`) |
| Minimum ${OS_NAME} | ${MIN_OS_ROW} |
| Source | ${SOURCE_URL} |
| Changelog | ${CHANGELOG_URL} |

### Upstream changelog ${EXCERPT_HEADING}
RevenueCat SDK section only (RevenueCatUI is not bound), without CI and tooling entries.

${EXCERPT_BLOCK}

### Native API changes
${API_BLOCK}

### Verification
- ${BUILD_LINE}
- ${WRAPPER_LINE}
- ⚠️ Not verified here: the demo harness on a device. Run \`demo/DemoApp\` on the platform before publishing.

### Binding sources
${FIX_BLOCK}

${RUN_LINE}

### Status
${STATUS_LINE}
EOF
)

# GitHub rejects a PR body over 65536 characters. The API report is what grows, and its
# full version is an artifact on the run, so the body is cut rather than the PR failing.
BODY_LIMIT=60000
if (( ${#BODY} > BODY_LIMIT )); then
  BODY="${BODY:0:$BODY_LIMIT}"$'\n\n'"_(truncated - see the \`api-report-${PLATFORM}\` artifact on the run)_"
fi

LABELS=(bump "$PLATFORM" dependencies)
if [[ "$CLAUDE_FIXED" == "true" ]]; then LABELS+=(claude-fixed); fi
if [[ "$STATUS" != "green" ]]; then LABELS+=("$STATUS"); fi

BODY_FILE=$(mktemp)
printf '%s\n' "$BODY" > "$BODY_FILE"

if [[ "$DRY_RUN" == "1" ]]; then
  echo "[dry-run] git push --force origin HEAD:refs/heads/${BRANCH}"
  echo "[dry-run] PR '${TITLE}'  ${BRANCH} -> ${BASE_BRANCH}  status=${STATUS}  labels=${LABELS[*]}"
  echo "----- PR body -----"
  cat "$BODY_FILE"
  echo "-------------------"
  exit 0
fi

# --- 4. Push (force: a previous run may have left the branch behind) --------------------
git push --force origin "HEAD:refs/heads/${BRANCH}"

# --- 5. Labels (idempotent) ------------------------------------------------------------------
gh label create bump              --color 0E8A16 --description "Native RevenueCat SDK bump produced by TryBumpAndroid / TryBumpIOS" --force >/dev/null
gh label create claude-fixed      --color 5319E7 --description "Binding sources were repaired by Claude Code - review them" --force >/dev/null
gh label create binding-broken    --color B60205 --description "The binding does not build against the new native SDK" --force >/dev/null
gh label create wrapper-broken    --color B60205 --description "The wrapper does not compile against the new binding" --force >/dev/null
gh label create min-os-raised     --color D93F0B --description "The native SDK raised the minimum OS version - breaking for consumers" --force >/dev/null
gh label create api-rebound       --color B60205 --description "A binding member keeps its C# signature but calls a different native method" --force >/dev/null
gh label create upstream-breaking --color D93F0B --description "RevenueCat marked a breaking change in this range, or it is a new major" --force >/dev/null

# The platform and dependencies labels are shared with people and Dependabot, so they are
# only created when missing, never overwritten. gh pr create fails on a missing label.
EXISTING_LABELS=$(gh label list --limit 1000 --json name --jq '.[].name')
create_missing_label() {
  # GitHub label names are case-insensitive: an existing "Android" blocks creating "android".
  if ! grep -qixF "$1" <<< "$EXISTING_LABELS"; then
    gh label create "$1" --color "$2" --description "$3" >/dev/null
  fi
}
create_missing_label android      EDEDED "Android binding"
create_missing_label ios          EDEDED "iOS binding"
create_missing_label dependencies 0366D6 "Pull requests that update a dependency file"

LABEL_ARGS=()
for label in "${LABELS[@]}"; do LABEL_ARGS+=(--label "$label"); done
ADD_LABEL_ARGS=()
for label in "${LABELS[@]}"; do ADD_LABEL_ARGS+=(--add-label "$label"); done

# --- 6. Create or refresh the PR ---------------------------------------------------------------
NUM=$(gh pr list --head "$BRANCH" --base "$BASE_BRANCH" --state open --json number --jq '.[0].number // empty')
if [[ -z "$NUM" ]]; then
  # macOS runners ship bash 3.2, where expanding an EMPTY array under `set -u` is an
  # "unbound variable" error; the ${arr[@]+"${arr[@]}"} idiom is safe on every bash.
  DRAFT_ARGS=()
  if [[ "$STATUS" != "green" ]]; then DRAFT_ARGS=(--draft); fi
  URL=$(gh pr create --base "$BASE_BRANCH" --head "$BRANCH" --title "$TITLE" --body-file "$BODY_FILE" \
    "${LABEL_ARGS[@]}" ${DRAFT_ARGS[@]+"${DRAFT_ARGS[@]}"})
  NUM=$(printf '%s' "$URL" | grep -oE '[0-9]+$')
  echo "==> opened PR #$NUM"
else
  echo "==> refreshing existing PR #$NUM"
  gh pr edit "$NUM" --title "$TITLE" --body-file "$BODY_FILE" \
    --remove-label claude-fixed --remove-label binding-broken --remove-label wrapper-broken \
    --remove-label min-os-raised --remove-label api-rebound --remove-label upstream-breaking >/dev/null || true
  gh pr edit "$NUM" "${ADD_LABEL_ARGS[@]}" >/dev/null
  if [[ "$STATUS" == "green" ]]; then
    gh pr ready "$NUM" || true
  else
    gh pr ready "$NUM" --undo || true
  fi
  URL=$(gh pr view "$NUM" --json url --jq .url)
fi

if [[ -z "$NUM" ]]; then
  echo "ERROR: could not determine a PR number for $BRANCH" >&2
  exit 1
fi

# --- 7. Close bump PRs this one supersedes ---------------------------------------------------
# RevenueCat ships several releases a week, so a newer one regularly arrives while the
# previous bump PR is still open. Both are opened against the pin on the base branch, so
# they coexist and the older one is dead weight.
#
# Two deliberate limits:
#   - only when THIS PR is green: a draft (broken build, breaking release) must not close an
#     older bump that is actually mergeable.
#   - only strictly older versions, so an explicitly dispatched older bump never closes a
#     newer one.
# The branch is deleted only when the superseded PR is an untouched single bot commit;
# anything else may carry a fix worth cherry-picking, and closing already makes it inert.
if [[ "$STATUS" == "green" ]]; then
  gh pr list --state open --base "$BASE_BRANCH" --json number,headRefName,isDraft \
      --jq ".[] | select(.headRefName | startswith(\"bump/${PLATFORM}-\")) | select(.number != ${NUM}) | \"\(.number) \(.headRefName) \(.isDraft)\"" \
    | while read -r old_num old_branch old_draft; do
        # Every command in this loop reads from /dev/null: the loop's stdin is the PR list,
        # and a command that reads stdin would swallow the PRs after the current one.
        old_version="${old_branch##*-}"
        if [[ "$(dotnet run .github/scripts/bump.cs -- compare-versions "$old_version" "$TARGET" < /dev/null)" != "-1" ]]; then
          echo "==> leaving PR #${old_num} (${old_version}) open: not older than ${TARGET}"
          continue
        fi

        commits=$(gh pr view "$old_num" --json commits --jq '.commits | length' < /dev/null)
        if [[ "$old_draft" == "false" && "$commits" == "1" ]]; then
          delete_note="Its branch \`${old_branch}\` has been deleted."
          delete_flag=(--delete-branch)
        else
          delete_note="Its branch \`${old_branch}\` is kept, because it carries work beyond the original bump commit."
          delete_flag=()
        fi

        echo "==> superseding PR #${old_num} (${old_version} -> ${TARGET})"
        gh pr close "$old_num" --comment "Superseded by ${URL} - RevenueCat ${PLATFORM_NAME} \`${TARGET}\` is now the newest release upstream, so this bump to \`${old_version}\` will not be merged. ${delete_note}" \
          ${delete_flag[@]+"${delete_flag[@]}"} < /dev/null || echo "WARN: could not close PR #${old_num}" >&2
      done
fi

if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  {
    echo "pr_number=${NUM}"
    echo "pr_url=${URL}"
    echo "branch=${BRANCH}"
  } >> "$GITHUB_OUTPUT"
fi
echo "==> PR #${NUM} ${URL} (${STATUS})"
