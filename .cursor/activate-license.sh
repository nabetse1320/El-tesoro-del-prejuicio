#!/usr/bin/env bash
# Activates a Unity license using whichever secrets are provided.
#
# Supported (in priority order):
#   1. UNITY_LICENSE  - contents of a Personal (free) .ulf license file.
#                       May be the raw XML or base64-encoded.
#   2. UNITY_SERIAL + UNITY_EMAIL + UNITY_PASSWORD - Unity Plus/Pro serial activation.
#
# See .cursor/README-unity-env.md for how to obtain a .ulf file for a free
# Personal license using the generated .alf activation request.
set -uo pipefail

UNITY_ROOT="/opt/unity"
UNITY_BIN="${UNITY_ROOT}/Editor/Unity"
ULF_SYS_DIR="/usr/share/unity3d/Unity"
ULF_SYS_PATH="${ULF_SYS_DIR}/Unity_lic.ulf"

log() { echo "[activate-license] $*"; }

if [ -f "${ULF_SYS_PATH}" ]; then
  log "License file already present at ${ULF_SYS_PATH}"
  exit 0
fi

# --- Personal (.ulf) license --------------------------------------------------
if [ -n "${UNITY_LICENSE:-}" ]; then
  log "Installing Personal license from UNITY_LICENSE secret..."
  sudo mkdir -p "${ULF_SYS_DIR}"
  if printf '%s' "${UNITY_LICENSE}" | grep -q "<root"; then
    printf '%s' "${UNITY_LICENSE}" | sudo tee "${ULF_SYS_PATH}" >/dev/null
  else
    printf '%s' "${UNITY_LICENSE}" | base64 -d | sudo tee "${ULF_SYS_PATH}" >/dev/null
  fi
  log "Wrote ${ULF_SYS_PATH}"
  exit 0
fi

# --- Plus/Pro serial activation ----------------------------------------------
if [ -n "${UNITY_SERIAL:-}" ] && [ -n "${UNITY_EMAIL:-}" ] && [ -n "${UNITY_PASSWORD:-}" ]; then
  log "Activating Unity via serial (Plus/Pro)..."
  xvfb-run -a "${UNITY_BIN}" \
    -batchmode -nographics -quit \
    -serial "${UNITY_SERIAL}" \
    -username "${UNITY_EMAIL}" \
    -password "${UNITY_PASSWORD}" \
    -logFile /dev/stdout
  exit $?
fi

log "No Unity license secrets found."
log "Set UNITY_LICENSE (.ulf contents) for a free Personal license, or"
log "UNITY_SERIAL + UNITY_EMAIL + UNITY_PASSWORD for Plus/Pro."
exit 0
