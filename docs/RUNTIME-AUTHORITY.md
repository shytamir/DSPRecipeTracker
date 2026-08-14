# Runtime Authority and Provenance

This document defines the evidence hierarchy for Dyson Sphere Program runtime
facts used by DSP Recipe Tracker. It records the current authoritative source
snapshot and prevents derived exports, inferred API shapes, or stale game
files from being treated as interchangeable.

## Authority hierarchy

Use sources in this order:

1. The installed game's `Assembly-CSharp.dll` with the exact SHA-256 recorded
   below is the primary authority for types, members, signatures, inheritance,
   method bodies, IL call sites, and runtime API behavior encoded in that
   assembly.
2. The hash-linked Phase 1 export bundle is authoritative derived evidence for
   the runtime Proto records and graph relationships that it preserves.
3. Direct inspection or a new focused export from the same assembly resolves
   any fact that is absent, ambiguous, summarized, or deferred in the bundle.
4. Project documentation and implementation are consumers of those sources;
   they do not override them.

Community documentation, wiki content, names inferred from UI presentation,
and source-defined compile shims may help investigation, but they do not
establish runtime authority.

BepInEx loader and Harmony authority is maintained separately in
[`BEPINEX-CONFORMANCE.md`](BEPINEX-CONFORMANCE.md). Do not infer loader support
from the game-assembly authority chain.

## Current assembly snapshot

| Field | Value |
| --- | --- |
| Installed source | `C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll` |
| Retained local copy | `artifacts/authority/installed/Assembly-CSharp.dll` |
| Size | 7,830,016 bytes |
| SHA-256 | `ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85` |
| Assembly identity | `Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null` |

The retained assembly is an unmodified copy of the installed source. Its hash
was verified after copying. It is a local inspection input only and must never
be committed, packaged, or redistributed.

## Current derived export snapshot

| Field | Value |
| --- | --- |
| Supplied bundle | `dsp_phase1_end_products_v1_0.zip` |
| Retained bundle | `artifacts/authority/bundles/dsp_phase1_end_products_v1_0.zip` |
| Extracted files | `artifacts/authority/exports/dsp_phase1_end_products_v1_0/` |
| Bundle size | 97,354 bytes |
| Attached bundle SHA-256 | `282720387dffc1e5fea4bffd72f0aa405392c545c05989e554603059e45ee3ae` |
| Export time recorded by bundle | `2026-07-19T01:51:31.9958668Z` |
| Validation status | `PASS`, with no errors or warnings |
| Assembly SHA-256 recorded by bundle | `ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85` |

The bundle's recorded assembly hash exactly matches both the installed and
retained assembly. This establishes that the export and direct assembly
inspection refer to the same runtime binary.

The validation document also records
`source_zip_sha256=6253ab971c284f19cd1b7bb0e5727a83197fcf75bbcf90cd847d67d27ff6a6f5`.
That value identifies the export process's upstream source ZIP; it is not the
SHA-256 of the attached six-file bundle. Keep the two hashes distinct.

## Bundle contents and coverage

The retained bundle contains:

- `dsp_phase1_canonical_dataset_v1_0.json`;
- `dsp_phase1_validation_v1_0.json`;
- `dsp_phase1_nodes_v1_0.csv`;
- `dsp_phase1_edges_v1_0.csv`;
- `dsp_phase1_milestone_seed_graph_v1_0.csv`; and
- `dsp_phase1_reconstruction_report_v1_0.md`.

All six extracted files were verified byte-for-byte against their archive
entries. The validation reports 314 technologies, 161 recipes, 174 items, 25
themes, and 14 vein types. IDs are unique, referenced graph nodes resolve, and
the formal technology graph is acyclic.

For this project, the bundle is the first source for recipe identities, direct
inputs and outputs, unlock relationships, item identities, and other fields it
preserves exactly. Practical-interpretation edges remain separate from formal
game-data edges and must not be promoted to runtime facts by inference.

The bundle does not contain a dedicated IL-disassembly or exhaustive call-site
inventory. It preserves some raw function ID/value pairs while explicitly
deferring their semantic interpretation. Use direct assembly inspection when
implementation requires IL references, exact member signatures, overload
selection, control flow, or a call pattern not proven by the exported data.

## Use and refresh rules

- Verify the installed assembly SHA-256 before using this snapshot for new
  runtime claims.
- If the installed hash differs, treat this snapshot as historical. Do not mix
  facts from the new assembly with exports tied to the old hash.
- Regenerate or supplement exports from the changed assembly before adopting
  new recipe or API claims.
- Record exact types, members, signatures, and inspected call sites for every
  runtime integration decision.
- Preserve unknowns as unknowns. Do not fill export gaps from intuition,
  partial shims, or unrelated game versions.
- Compile-time references must satisfy the integrity rules in
  [`THUNDERSTORE-PACKAGE.md`](THUNDERSTORE-PACKAGE.md).
- Technical validation against these sources remains separate from in-game
  owner acceptance.

## Sprint 1 compile-reference snapshot

Sprint 1 uses the following complete installed assemblies. This is the
authorized reference set for the plugin foundation; implementation must add a
new authority entry before consuming another Unity module.

| Relative to the game root | Assembly identity | SHA-256 |
| --- | --- | --- |
| `DSPGAME_Data\Managed\Assembly-CSharp.dll` | `Assembly-CSharp, Version=0.0.0.0` | `ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85` |
| `BepInEx\core\BepInEx.dll` | `BepInEx, Version=5.4.17.0` | `dc1cb6b58b962bda5aaa1d6b5f9ae14ec174f61836a1a1f96c1a040c7e8381f7` |
| `DSPGAME_Data\Managed\UnityEngine.dll` | `UnityEngine, Version=0.0.0.0` | `72cc73eef0036530abe21f82971ff06002cee37effdd4dd7d5d4ec8df3911f8d` |
| `DSPGAME_Data\Managed\UnityEngine.CoreModule.dll` | `UnityEngine.CoreModule, Version=0.0.0.0` | `e2b5ae2fd12646d03fc3d04d1a37d522572a3b97022fe1b95bbf2a2f2b04853a` |
| `DSPGAME_Data\Managed\UnityEngine.UIModule.dll` | `UnityEngine.UIModule, Version=0.0.0.0` | `c37bb3eace97302fb3aa9e17eac4446f8b010307637aa9cc5cb84c59828b0ab2` |
| `DSPGAME_Data\Managed\UnityEngine.UI.dll` | `UnityEngine.UI, Version=1.0.0.0` | `54953ebd7c9b4b39279876b37109f0f503938847f2a7be4a22d62e9b94c347eb` |

Set `DSP_GAME_DIR` to the supported game root and run the fail-closed preflight
before every Release build and installed checkpoint:

```powershell
$env:DSP_GAME_DIR = 'C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program'
.\scripts\Test-LocalAuthority.ps1
```

The command has no fallback search path. A missing environment value, missing
file, hash mismatch, or assembly-identity mismatch stops validation before
compilation. Repository `Release` builds inherit `Directory.Build.targets`,
which invokes this preflight before reference resolution. Installed checkpoint
commands must invoke the same script before copying or launching the plugin.

## Repository boundary

All retained authority inputs live under the ignored `artifacts/` directory.
They are intentionally absent from Git status and Git history. Tracked
documentation may record identities, hashes, provenance, and derived
conclusions, but must not embed or redistribute licensed game binaries or
large generated datasets.
