#!/usr/bin/env bash
# Idempotent Cloud Agent setup for the "El Tesoro del Prejuicio" Unity project.
# - Installs the OS libraries the Linux Unity Editor depends on.
# - Installs Unity Editor 2023.1.6f1 (matches ProjectSettings/ProjectVersion.txt).
# - Activates a Unity license when license secrets are available.
# - Warms the project Library (asset import + script compile) when licensed.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

UNITY_VERSION="2023.1.6f1"
UNITY_CHANGESET="964b2488c462"
UNITY_ROOT="/opt/unity"
UNITY_BIN="${UNITY_ROOT}/Editor/Unity"

log() { echo "[install] $*"; }

# 1. System libraries required by the Linux Unity Editor -----------------------
log "Installing system dependencies..."
sudo apt-get update -qq
sudo DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
  ca-certificates curl wget git xz-utils \
  xvfb \
  libgtk-3-0 libglu1-mesa libnss3 libxtst6 libxss1 libgbm1 libnotify4 \
  libxrandr2 libatk-bridge2.0-0 libx11-xcb1 libasound2t64 libcups2 \
  libncursesw6 libunwind8

# 2. Unity Editor 2023.1.6f1 ---------------------------------------------------
if [ -x "${UNITY_BIN}" ]; then
  log "Unity Editor already installed at ${UNITY_BIN}"
else
  log "Downloading Unity Editor ${UNITY_VERSION} (changeset ${UNITY_CHANGESET})..."
  sudo mkdir -p "${UNITY_ROOT}"
  sudo chown "$(id -u):$(id -g)" "${UNITY_ROOT}"
  curl -fSL --retry 4 --retry-delay 4 -o "${UNITY_ROOT}/Unity.tar.xz" \
    "https://download.unity3d.com/download_unity/${UNITY_CHANGESET}/LinuxEditorInstaller/Unity.tar.xz"
  log "Extracting Unity Editor..."
  tar -xf "${UNITY_ROOT}/Unity.tar.xz" -C "${UNITY_ROOT}"
  rm -f "${UNITY_ROOT}/Unity.tar.xz"
  log "Unity Editor installed at ${UNITY_BIN}"
fi

"${UNITY_BIN}" -version 2>/dev/null || true

# 3. License activation --------------------------------------------------------
"${SCRIPT_DIR}/activate-license.sh" || log "License activation skipped/failed (see messages above)."

# 4. Warm the project Library (only possible with a valid license) -------------
if "${SCRIPT_DIR}/license-active.sh"; then
  log "Importing project to warm the Library (this can take a few minutes)..."
  xvfb-run -a "${UNITY_BIN}" \
    -batchmode -nographics -quit \
    -projectPath "${PROJECT_DIR}" \
    -logFile /dev/stdout || log "Project import returned a non-zero exit code."
else
  log "No active Unity license; skipping project import."
  log "Provide license secrets (see .cursor/README-unity-env.md) and re-run setup."
fi

log "Install complete."
