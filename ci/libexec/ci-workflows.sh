#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Project CI Workflow Composition Library.
#   Contains functions which execute the project's high-level continuous integration tasks.
#
# How to use:
#   Update the "workflow compositions" in this file to perform each of the named continuous integration tasks.
#   Add additional workflow functions as needed. Note: Functions must be executed to affect CI process.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# Infer this script has been sourced based upon WORKFLOWS_SCRIPT_LOCATION being non-empty.
if [[ -n "${WORKFLOWS_SCRIPT_LOCATION:-}" ]]; then
  # Workflows are already sourced. Exit.
  # Check to see if this script was sourced.
  #   See: https://stackoverflow.com/a/28776166/402726
  (return 0 2>/dev/null) && sourced=1 || sourced=0
  if [[ $sourced -eq 1 ]]; then
    # NOTE: return is used, rather than exit, to prevent shell exit when sourcing from an interactive shell.
    return 0
  else
    exit 0
  fi
fi

# Context
WORKFLOWS_SCRIPT_LOCATION="${BASH_SOURCE[0]}"
declare WORKFLOWS_SCRIPT_DIRECTORY="$(dirname "${WORKFLOWS_SCRIPT_LOCATION}")"
PROJECT_ROOT="${PROJECT_ROOT:-$(cd "${WORKFLOWS_SCRIPT_DIRECTORY}" && cd ../.. && pwd)}"

# Load the CICEE continuous integration action library (local copy, by 'cicee lib', or by the specific location CICEE mounts it to).
if [[ -d "${PROJECT_ROOT}/ci/lib/ci/bash" ]]; then
  source "${PROJECT_ROOT}/ci/lib/ci/bash/ci.sh" && printf "Loaded local CI lib: ${PROJECT_ROOT}/ci/lib\n"
elif [[ -n "$(command -v cicee)" ]]; then
  source "$(cicee lib)" && printf "Loaded CICEE's CI lib.\n"
else
  # CICEE mounts the Bash CI action library at /opt/ci-lib/bash/ci.sh.
  source "/opt/ci-lib/bash/ci.sh" && printf "Loaded CICEE's mounted CI lib.\n"
fi

####
# BEGIN Workflow Compositions
#     These commands are executed by CI entrypoint scripts, e.g., publish.sh.
#     By convention, each CI workflow function begins with "ci-".
####

#--
# Validate the project's source, e.g. run tests, linting.
#--
ci-validate() {
  ci-dotnet-restore &&
    ci-dotnet-build &&
    ci-dotnet-test
}

#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  ci-dotnet-pack
}

#--
# Publish the project's artifact composition.
#--
ci-publish() {
  ci-dotnet-nuget-push
}

export -f ci-compose
export -f ci-publish
export -f ci-validate

####
# END Workflow Compositions
####
