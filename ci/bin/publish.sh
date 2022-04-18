#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Build and publish the project's artifact composition.
#
# How to use:
#   Customize the "ci-compose" and "ci-publish" workflows (functions) defined in ci-workflows.sh.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

declare SCRIPT_LOCATION="$(dirname "${BASH_SOURCE[0]}")"
declare PROJECT_ROOT="${PROJECT_ROOT:-$(cd "${SCRIPT_LOCATION}/../.." && pwd)}"

__initialize() {
  # Load the CICEE continuous integration action library (local copy, by 'cicee lib', or by the specific location CICEE mounts it to).
  if [[ -d "${PROJECT_ROOT}/ci/lib/ci/bash" ]]; then
    source "${PROJECT_ROOT}/ci/lib/ci/bash/ci.sh" && printf "Loaded local CI lib: ${PROJECT_ROOT}/ci/lib\n"
  elif [[ -n "$(command -v cicee)" ]]; then
    source "$(cicee lib)" && printf "Loaded CICEE's CI lib.\n"
  else
    # CICEE mounts the Bash CI action library at /opt/ci-lib/bash/ci.sh.
    source "/opt/ci-lib/bash/ci.sh" && printf "Loaded CICEE's mounted CI lib.\n"
  fi
  # Load project CI workflow library.
  # Then execute the ci-env-init, ci-env-display, and ci-env-require functions, provided by the CICEE action library.
  source "${PROJECT_ROOT}/ci/libexec/ci-workflows.sh" &&
    ci-env-init &&
    ci-env-display &&
    ci-env-require
}

# Execute the initialization function, defined above, and ci-compose and ci-publish functions, defined in ci/libexec/ci-workflows.sh.
__initialize &&
  printf "Composing build artifacts...\n\n" &&
  ci-compose &&
  printf "Composition complete.\n" &&
  printf "Publishing composed artifacts...\n\n" &&
  ci-publish &&
  printf "Publishing complete.\n\n"
