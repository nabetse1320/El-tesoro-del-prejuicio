#!/usr/bin/env bash
# Runs on every Cloud Agent boot: ensures a Unity license is active.
# License activation is intentionally here (not only in install) so that agents
# booting from a prebuilt environment snapshot still activate using the current
# secrets. It is safe to run repeatedly.
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

"${SCRIPT_DIR}/activate-license.sh" || true

if "${SCRIPT_DIR}/license-active.sh"; then
  echo "[start] Unity license active."
else
  echo "[start] No active Unity license. The editor is installed but cannot build/run"
  echo "[start] until license secrets are provided (see .cursor/README-unity-env.md)."
fi
