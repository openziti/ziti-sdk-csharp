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
# Safe to run locally; override the threshold with KEEPALIVE_MAX_AGE_DAYS.
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

echo "$entry" >> "$log_file"
git add "$log_file"

if git diff --cached --quiet; then
    echo "keepalive: nothing to commit"
    exit 0
fi

# Commit as the Actions bot without persisting identity into repo config.
git \
    -c user.name="github-actions[bot]" \
    -c user.email="41898282+github-actions[bot]@users.noreply.github.com" \
    commit -m "$commit_msg"
git push
