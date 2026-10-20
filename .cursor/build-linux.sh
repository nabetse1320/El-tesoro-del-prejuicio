#!/usr/bin/env bash
# Builds a Linux x86_64 standalone player for the project using a temporary
# editor build script. Requires an active Unity license.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
UNITY_BIN="/opt/unity/Editor/Unity"
OUTPUT="${PROJECT_DIR}/Build/Linux/ElTesoroDelPrejuicio.x86_64"

log() { echo "[build-linux] $*"; }

if ! "${SCRIPT_DIR}/license-active.sh"; then
  log "No active Unity license. Provide license secrets first (see README-unity-env.md)."
  exit 1
fi

# Install a temporary editor build method (removed after the build).
EDITOR_DIR="${PROJECT_DIR}/Assets/Editor"
BUILD_CS="${EDITOR_DIR}/CloudAgentBuild.cs"
mkdir -p "${EDITOR_DIR}"
cat > "${BUILD_CS}" <<'CS'
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CloudAgentBuild
{
    public static void BuildLinux()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Build/Linux/ElTesoroDelPrejuicio.x86_64",
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[CloudAgentBuild] Result={summary.result} " +
                  $"TotalSize={summary.totalSize} " +
                  $"Errors={summary.totalErrors} " +
                  $"Warnings={summary.totalWarnings}");

        if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
        EditorApplication.Exit(0);
    }
}
CS

cleanup() { rm -f "${BUILD_CS}" "${BUILD_CS}.meta"; }
trap cleanup EXIT

log "Building Linux standalone player..."
xvfb-run -a "${UNITY_BIN}" \
  -batchmode -nographics -quit \
  -projectPath "${PROJECT_DIR}" \
  -executeMethod CloudAgentBuild.BuildLinux \
  -logFile /dev/stdout

if [ -f "${OUTPUT}" ]; then
  log "Build succeeded: ${OUTPUT}"
else
  log "Build finished but expected output not found: ${OUTPUT}"
  exit 1
fi
