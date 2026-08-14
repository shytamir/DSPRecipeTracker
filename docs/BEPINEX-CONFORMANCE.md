# BepInEx Conformance Contract

This document fixes the BepInEx loader and plugin-manager contract supported
by DSP Recipe Tracker. The project conforms to the version already installed
and operating with the default Steam game. It does not modify BepInEx, select
an upgrade path, or promise compatibility with other loader versions.

**Evidence disposition:** The former retained local snapshot was removed. The
owner accepts the recorded identities, hashes, source mapping, and conformance
conclusions in this document as the recovered baseline.

## Supported version

| Field | Value |
| --- | --- |
| Installed BepInEx assembly | `BepInEx 5.4.17.0` |
| Installed source tag | `v5.4.17` |
| Source commit | `3c3153598b530fb85e623ba226c8cca1b3f4d262` |
| Source checkout | `D:\Shy\BepInEx` |
| HarmonyX package | `2.5.5` |
| Installed HarmonyX assembly | `0Harmony 2.5.5.0` |
| BepInEx.Harmony gitlink | `adcde1d95ef054cbbc5c88e75526859c82621e94` |
| Unity Doorstop | `3.4.0.0` |
| Thunderstore dependency | `xiaoye97-BepInEx-5.4.17` |

The source checkout's current `master` is BepInEx 6 development at commit
`6abdba47eeebe08552282e7a58ef0f4a9ab60b62`. That checkout state is not the
supported source contract. The locally available `v5.4.17` tag and its exact
commit above are authoritative for the installed BepInEx 5 API.

The project-owned loader identity approved in `PROJECT.md` is GUID
`dsprecipetracker` with display name `DSP-Recipe-Tracker`. This identity is
independent of the loader-version conformance evidence in this file.

## Installed runtime identity

The recorded binary identities are:

| File | Assembly/file version | SHA-256 |
| --- | --- | --- |
| `BepInEx.dll` | `5.4.17.0` | `dc1cb6b58b962bda5aaa1d6b5f9ae14ec174f61836a1a1f96c1a040c7e8381f7` |
| `BepInEx.Preloader.dll` | `5.4.17.0` | `adc41c73790ae5363217c8d0b178a02ccb85c17925810e735f4bc1564315f01c` |
| `0Harmony.dll` | `2.5.5.0` | `7bd2bd6f87c1758047def40f2f0f024c877456ce7c01d68031358ee0c615d850` |
| `BepInEx.Harmony.dll` | `2.0.0.0` | `d0739c4a13f369094cb164c205ee4cca5392bdd7241b9f242ee13f0d4c0b1856` |
| `HarmonyXInterop.dll` | `1.0.0.0` | `e818d779a52b4d779c6dc873f3e4b19287abbc948575629ab58d8252ff8955f2` |
| `Mono.Cecil.dll` | `0.10.4.0` | `7ae470288fff4a402899c254d0a76cefef55877f5c54f96e83c797cc5bb6e2f6` |
| `MonoMod.RuntimeDetour.dll` | `21.9.19.1` | `281bfb29c5e9cc4cb98b81e7afc0171ae12891a7b7370a98568d9e2a3060db50` |
| `MonoMod.Utils.dll` | `21.9.19.1` | `4cc34a5c4278d78ce3f516bb3b43c9a5ed3509672dbba932e036746a9360a570` |
| `winhttp.dll` | `3.4.0.0` | `cf9dd372ca0ddbe01153502c49f8f756197bb260001792fe766f6c0242dc7fc0` |

The `v5.4.17` source declares HarmonyX `2.5.5`, Mono.Cecil `0.10.4`, and
MonoMod RuntimeDetour and Utils `21.9.19.1`. Those declarations match the
installed dependency closure.

`doorstop_config.ini` enables Doorstop and targets
`BepInEx\core\BepInEx.Preloader.dll`. Its recorded SHA-256 is
`2255e7640434fdfccbfeb123a5f4fccb05032481b39c2ba822e905ccba58d20e`.

## Removed local authority snapshot

The former ignored snapshot contained the 18 files from `BepInEx/core`, the
Doorstop DLL and configuration, recorded hashes, and a source archive produced
from the pinned tag. Those files were removed. The recorded source-archive
SHA-256 was
`43154c79b2bcafe5978429df19a3076528471c2406ec923eaba185e68c1d6d26`.

The owner accepts the conclusion that the snapshot matched the active
installation and pinned source mapping at the time of recovery. If new Harmony
internals require inspection, use the exact recorded submodule commit or a new
explicitly established source; do not substitute current upstream code by
inference.

## Minimal supported feature surface

The plugin may use only the BepInEx features required by its implementation:

- `[BepInPlugin]` for stable plugin identity and semantic version metadata;
- `BaseUnityPlugin` for the Unity Mono plugin lifecycle;
- the plugin-tied `ManualLogSource` exposed by `BaseUnityPlugin.Logger`;
- the tied `ConfigFile` only if explicit user-configurable settings are added;
  and
- HarmonyX's public `HarmonyLib` patching API only when a confirmed game hook
  cannot be implemented through an existing stable lifecycle or event.

Harmony use must have one stable owner ID, patch only confirmed target methods,
and remove this plugin's patches during shutdown. The need for Harmony must be
established by the runtime integration design; its presence in the installed
loader is not by itself a reason to patch.

The plugin must not depend directly on preloader APIs, Chainloader internals,
Cecil, MonoMod detours, HarmonyXInterop, legacy `0Harmony20.dll`, or unrelated
plugin-manager services. Those files are part of the installed loader closure,
not the project's supported consumption surface.

## Fixed-version policy

- Support exactly the installed BepInEx `5.4.17.0` contract.
- Do not fork, patch, rebuild, replace, or redistribute the plugin manager.
- Do not add multi-version adapters, loader abstraction, version probing, or
  compatibility fallbacks without a separate owner decision.
- Local builds compile against the installed `BepInEx.dll`; hosted builds use
  the exact documented Thunderstore dependency. Reference `0Harmony.dll` only
  if the implementation actually uses HarmonyX.
- Hosted CI may acquire the exact `xiaoye97-BepInEx-5.4.17` package only when
  it verifies the consumed assembly identity against this accepted conformance
  baseline.
- Do not compile against the BepInEx 6 `master` checkout state.
- Treat an installed BepInEx or relevant dependency hash change as a
  conformance change requiring renewed source validation and, only where the
  change creates an otherwise-unprovable runtime claim, a later owner-performed
  human check.
- A compile-only result does not establish installed compatibility. Discovery,
  lifecycle, logging, configuration, patch ownership, shutdown cleanup, and
  coexistence remain unvalidated until the owner performs the applicable
  bounded human checks against a supplied testable build. Agents do not install
  or load the project plugin in the game environment.

The version in use is the version supported. If it stops satisfying project
requirements, that is a new authority and product decision rather than an
implicit loader upgrade.

## Recorded historical loader observation

The removed historical `BepInEx/LogOutput.log`, recorded as last written at
`2026-08-14T08:36:03.3471257Z`, reported:

- BepInEx `5.4.17.0` starting for DSPGAME;
- one patcher plugin loaded;
- three plugins selected for loading; and
- `Chainloader startup complete`.

The owner accepts the conclusion that the pinned loader started and coexisted
with the observed installed plugins at that time. This does not establish a
current environment state or any DSP Recipe Tracker behavior.
