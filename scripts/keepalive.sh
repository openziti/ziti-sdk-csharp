#!/usr/bin/env bash
# Keepalive + publish log for the native nightly. GitHub disables scheduled workflows after 60 days with no
# repository activity, and scheduled runs themselves do NOT count -- so without this the nightly would eventually
# stop firing on its own. It runs at the end of every nightly build:
#   * --published <version>: a native package shipped this run; append it to the log (which also resets the clock).
#   * no args: nothing shipped; append a heartbeat ONLY if the repo has been quiet >= KEEPALIVE_MAX_AGE_DAYS
#     (default 50), so a long ziti-sdk-c dry spell can't let the cron lapse.
#
# Staleness is measured off the most recent commit, so ANY activity (a merge, a publish log entry) resets the
# clock and the heartbeat only fires when the repo is genuinely quiet. A committed file change is used on purpose:
# empty commits and tag pushes don't reliably count as activity.
#
# The commit is written through the GitHub contents API rather than git push, because main requires verified
# signatures. Needs gh with a token carrying contents:write; override the target with KEEPALIVE_BRANCH.
#
# Override the threshold with KEEPALIVE_MAX_AGE_DAYS. Running it with no args on a repo that is not yet stale
# exits before touching anything, which makes it safe to try by hand.
#
# Usage:
#   ./scripts/keepalive.sh                        # heartbeat if the repo is going stale
#   ./scripts/keepalive.sh --published 1.18.2.50  # log a publish

set -euo pipefail

max_age_days="${KEEPALIVE_MAX_AGE_DAYS:-50}"
log_file="native-publish.log"

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/.." && pwd)"
cd "$repo_root"

published_version=""
if [ "${1:-}" = "--published" ]; then
    published_version="${2:-}"
fi

now_iso="$(date -u +%Y-%m-%dT%H:%M:%SZ)"

if [ -n "$published_version" ]; then
    entry="${now_iso}  published OpenZiti.NET.native ${published_version}"
    commit_msg="keepalive: log native publish ${published_version}"
else
    # Age of the most recent commit, in whole days. actions/checkout fetches only the tip, which is all this needs.
    last_commit_epoch="$(git log -1 --format=%ct)"
    now_epoch="$(date -u +%s)"
    age_days=$(( (now_epoch - last_commit_epoch) / 86400 ))
    if (( age_days < max_age_days )); then
        echo "keepalive: last commit ${age_days}d ago (< ${max_age_days}d); nothing to do"
        exit 0
    fi
    entry="${now_iso}  heartbeat -- no new ziti-sdk-c release in ${age_days}d"
    commit_msg="keepalive: repo heartbeat (reset 60-day inactivity clock)"
fi

# main requires verified signatures, and a runner has no signing key, so the write goes through the GitHub
# contents API instead of git push. GitHub signs those commits with its web-flow key when the caller is an app
# installation token, which the Actions GITHUB_TOKEN is. Running this by hand with a personal access token
# produces an UNSIGNED commit, so a local run proves the file write but not the signature.
repo="${GITHUB_REPOSITORY:-$(gh repo view --json nameWithOwner --jq .nameWithOwner)}"
branch="${KEEPALIVE_BRANCH:-main}"

# Read the file as it exists on the branch. A 404 means the log has never been written, which is normal on a
# fresh repo, so fall through to creating it. Note gh prints its error BODY to stdout, so the response is only
# parsed when the call actually succeeded -- otherwise the error json gets mistaken for content.
if existing_json="$(gh api "repos/${repo}/contents/${log_file}?ref=${branch}" 2>/dev/null)"; then
    existing_sha="$(printf '%s' "$existing_json" | jq -r '.sha // empty')"
    existing_content="$(printf '%s' "$existing_json" | jq -r '.content // empty' | tr -d '\n' | base64 -d)"
    new_content="${existing_content}${entry}"$'\n'
else
    echo "keepalive: ${log_file} not on ${branch} yet; creating it"
    existing_sha=""
    new_content="${entry}"$'\n'
fi

encoded="$(printf '%s' "$new_content" | base64 -w0)"

echo "keepalive: committing to ${repo}@${branch}: ${commit_msg}"
if [ -n "$existing_sha" ]; then
    gh api -X PUT "repos/${repo}/contents/${log_file}" \
        -f message="$commit_msg" \
        -f content="$encoded" \
        -f branch="$branch" \
        -f sha="$existing_sha" \
        --jq '.commit.sha'
else
    gh api -X PUT "repos/${repo}/contents/${log_file}" \
        -f message="$commit_msg" \
        -f content="$encoded" \
        -f branch="$branch" \
        --jq '.commit.sha'
fi
