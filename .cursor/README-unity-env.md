# Cloud Agent environment — El Tesoro del Prejuicio (Unity 2023.1.6f1)

This directory configures the Cloud Agent development environment for this Unity
project.

## What gets installed

`install.sh` runs once during environment setup and:

1. Installs the OS libraries the Linux Unity Editor needs (GTK, NSS, GL, etc.).
2. Installs **Unity Editor 2023.1.6f1** (changeset `964b2488c462`, matching
   `ProjectSettings/ProjectVersion.txt`) into `/opt/unity`.
3. Activates a Unity license from secrets (see below).
4. Warms the project `Library/` (imports assets + compiles scripts) when a
   license is active.

`start.sh` re-activates the license on every boot so agents that start from a
prebuilt snapshot pick up the current license secrets.

## License (required to build or run)

Unity refuses to launch in batch mode without a license. Add **one** of the
following via the Secrets panel:

### Option A — free Personal license (recommended)

Provide the contents of a `.ulf` license file as the `UNITY_LICENSE` secret.

To obtain the `.ulf`:

1. Download `Unity_v2023.1.6f1.alf` (a manual activation request generated on
   this machine — it is attached to the agent run, and can also be regenerated
   with `xvfb-run -a /opt/unity/Editor/Unity -batchmode -nographics -quit -createManualActivationFile`).
2. Go to <https://license.unity3d.com/manual>, sign in with a Unity account, and
   upload the `.alf`.
3. Choose **Unity Personal → I don't use Unity in a professional capacity**.
4. Download the resulting `Unity_v2023.x.ulf` file.
5. Paste the full XML contents of that file into a secret named `UNITY_LICENSE`
   (base64 is also accepted).

### Option B — Unity Plus/Pro serial

Provide all three secrets: `UNITY_SERIAL`, `UNITY_EMAIL`, `UNITY_PASSWORD`.

## Building / running the game

The game targets a desktop standalone player. From the workspace root:

```bash
./.cursor/build-linux.sh      # builds a Linux x86_64 standalone player
```

The output is written to `Build/Linux/`. Open the project interactively, run
edit-mode/play-mode tests, or build for other platforms with the editor at
`/opt/unity/Editor/Unity`.
