# BepInEx Conformance Contract

This document fixes the BepInEx loader and plugin-manager contract supported
by DSP Recipe Tracker. The project conforms to the version already installed
and operating with the default Steam game. It does not modify BepInEx, select
an upgrade path, or promise compatibility with other loader versions.

**Evidence disposition:** The former retained local snapshot was removed. The
owner accepts the recorded version, source mapping, and conformance conclusions
in this document as the recovered baseline.

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

## Dependency identity and acquisition

The `v5.4.17` source declares HarmonyX `2.5.5`, Mono.Cecil `0.10.4`, and
MonoMod RuntimeDetour and Utils `21.9.19.1`. Those declarations match the
accepted installed dependency closure.

Hosted CI downloads the documented Thunderstore dependency and performs one
checksum comparison before using its `BepInEx.dll`. That check protects the
remote dependency acquisition; it is not game-runtime authority and is not
repeated by local or story validation. Local and hosted builds otherwise
confirm the required BepInEx assembly version and consumed compile surface.

If new Harmony internals require inspection, use the exact recorded submodule
commit or a new explicitly established source; do not substitute current
upstream code by inference.

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
- Treat a supported BepInEx version or consumed-surface change as a conformance
  change requiring renewed source validation and, only where it creates an
  otherwise-unprovable runtime claim, a later owner-performed human check.
- A compile-only result does not establish installed compatibility. The owner
  validated discovery, lifecycle, logging, shutdown cleanup, and coexistence
  for the recorded MVP build and conditions. Configuration and patch ownership
  were not consumed by that implementation, and broader compatibility remains
  unclaimed. Agents do not install or load the project plugin in the game
  environment.

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
