#!/usr/bin/env bash
# Exits 0 when the Unity Editor reports a usable license, non-zero otherwise.
# Opens a throwaway empty project so the check is fast; the editor performs the
# license check before doing any project work and logs a clear error when no
# valid license is available.
set -uo pipefail

UNITY_BIN="/opt/unity/Editor/Unity"
LOG="$(mktemp)"
TMP_PROJ="$(mktemp -d)"

xvfb-run -a "${UNITY_BIN}" -batchmode -nographics -quit \
  -projectPath "${TMP_PROJ}" -logFile "${LOG}" >/dev/null 2>&1 || true

status=0
if grep -qi "No valid Unity Editor license found" "${LOG}"; then
  status=1
fi

rm -rf "${LOG}" "${TMP_PROJ}"
exit "${status}"
