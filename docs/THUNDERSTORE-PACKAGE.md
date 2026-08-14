# DSP Recipe Tracker - Thunderstore Package Contract

## Status and authority

**Status:** Current package contract

**Implementation status:** The S1-03 package pipeline is implemented,
technically validated, and owner accepted. It creates an inspected development
artifact only; no supported or published release exists.

This contract defines package layout, version mapping, build inputs, static
validation, and artifact retention for DSP Recipe Tracker. It does not publish,
promote, or authorize a Thunderstore release. Product claims remain governed
by [`PROJECT.md`](PROJECT.md), implementation by
[`IMPLEMENTATION-CONTRACT.md`](IMPLEMENTATION-CONTRACT.md), validation claims
by [`VALIDATION-CONTRACT.md`](VALIDATION-CONTRACT.md), and active scope by
[`ROADMAP.md`](ROADMAP.md).

## 1. ZIP layout

```text
manifest.json
README.md
icon.png
BepInEx/
  plugins/
    DSPRecipeTracker/
      DSPRecipeTracker.dll
```

The three Thunderstore metadata files are placed at the ZIP root with exact
case. The single recipe-tracker assembly is installed under the intended
BepInEx plugin path.

The archive excludes:

- DSP, Unity, BepInEx, Harmony, and build-reference binaries;
- PDBs, logs, saves, probes, screenshots, test output, and validation fixtures;
- source, intermediate build files, and CI scripts; and
- any DLL or asset not explicitly listed above.

No placeholder, zero-byte, or installable-looking dummy package is permitted.

## 2. Version mapping

`VERSION` supplies the manually selected major (`M`) and minor (`m`) values.
The GitHub Actions run number supplies the automatically increasing patch
value (`N`):

```text
Package/plugin version: M.m.N
Semantic version:       M.m.N
Assembly/file version:  M.m.N.0
Diagnostic label:       M.m.N.<short-commit>
```

The current pre-release line is `0.1`. The same generated semantic version is
used by `manifest.json` and the BepInEx plugin attribute. The recipe-tracker
assembly uses the generated four-part assembly and file version. The diagnostic
commit suffix is retained in build information and product diagnostics without
changing loader identity or semantic version.

`VERSION` is the only manually edited release-line source. The pipeline must
not maintain separate hard-coded versions in project, manifest, or package
scripts.

## 3. Build and validation workflow

Sprint 1 implements local package construction and GitHub Actions automation
using the same static validators. A workflow triggered by each push to `main`
and by manual dispatch will:

1. verify that it checked out the triggering commit;
2. derive all version forms from `VERSION`, the run number, and commit;
3. build the tracked game and Unity compile-reference shims and validate their
   declaration-only surface against the tracked inventory;
4. build the real `net472` plugin against those shims and run the implemented
   deterministic tests;
5. extract the production assembly's external references and validate their
   complete coverage by both the shim declarations and surface inventory;
6. validate that `DSPRecipeTracker.dll` is a non-empty managed assembly with
   the expected name, semantic metadata, assembly version, file version, and
   BepInEx plugin identity;
7. build the Thunderstore ZIP from real build output and tracked package
   assets;
8. validate the exact archive allowlist, portable names, root files, manifest
   shape, semantic version, README presence, PNG format and dimensions, DLL
   path, and recorded DLL hash; and
9. upload the ZIP, recipe-tracker DLL, build information, and static validation
   reports as one workflow artifact retained for 30 days.

Local package construction receives an explicit non-negative patch value
corresponding to `N`; GitHub Actions supplies its run number. Neither path
edits `VERSION`.

The workflow does not publish to Thunderstore, create a GitHub release, or
describe the artifact as supported or release-ready.

Generic package validation checks structure, identity, integrity, and version
consistency. It does not claim that recipe tracking, UI, interaction, runtime
cleanup, compatibility, or any other product behavior works.

Plugin metadata validation requires the exact BepInEx GUID
`dsprecipetracker` and display name `DSP-Recipe-Tracker` recorded in
`PROJECT.md`. Package tooling must not derive or normalize either value from a
folder, assembly, repository, or Thunderstore package name.

## 4. Build prerequisites and reference boundary

- Local builds use an explicit `GameRoot` setting that points to the supported
  DSP installation. Game, Unity, and BepInEx assemblies are read as compile
  inputs and are never copied into source control or package output.
- Hosted CI may acquire the exact documented BepInEx `5.4.17` package when the
  consumed assembly identity matches the conformance contract.
- Hosted CI must not download, upload, cache, reconstruct, or redistribute
  licensed DSP or Unity binaries.
- Hosted CI uses tracked source-defined compile-reference shims for the exact
  DSP and Unity surface consumed by production code. They live under
  `ci/compile-references` with their `surface-inventory.json`. A shim contains
  type and member declarations only: no game logic, extracted implementation,
  copied binary content, or runtime simulation.
- The shim projects, their outputs, and their surface inventory are compile and
  validation inputs only. They are never installed or included in the package.
- The implemented automation uses the repository-selected .NET SDK and
  PowerShell tooling recorded by the active Sprint 1 story.

### Shim coverage validation

The repository maintains a machine-readable inventory of every known DSP and
Unity compile-reference consumed by production code. Coverage includes assembly
and type identities, base types and interfaces, constructors, methods, fields,
properties, events, and the parameter and return types needed by those members.

The validator extracts the external DSP and Unity type/member references from
the production `Release` assembly and compares them with both the inventory and
the declarations exported by the shim assemblies. It fails when:

- a consumed external reference is absent from either source;
- a declared name, containing type, static/instance form, parameter list,
  return type, field/property/event type, or assembly identity disagrees;
- a shim declaration falls outside the reviewed surface inventory;
- production source uses an external reference that the extractor cannot
  classify; or
- a shim exposes executable behavior or is present in package output.

External members reached only through reflection or other dynamic lookup do
not appear in ordinary assembly member references. Each such known access must
therefore have an explicit inventory entry and a focused source-level check.
Shim coverage proves that hosted compilation covers every known consumed
compile surface; it does not prove in-game behavior or compatibility.

## 5. Package assets

- `manifest.json` uses the generated semantic version and the package identity
  approved for DSP Recipe Tracker. Its runtime dependency is the supported
  Thunderstore BepInEx package for the fixed `5.4.17` loader line documented in
  `BEPINEX-CONFORMANCE.md`.
- `README.md` describes only behavior and readiness states supported by the
  current product and validation contracts.
- `icon.png` satisfies Thunderstore's required PNG format and dimensions and
  contains no copied game art.
- Package assets are tracked beneath `packaging/`; generated archives and
  binaries remain ignored build output.

## 6. Validation and publication boundary

Package construction and static inspection may establish only
`Package-inspected` status as defined by `VALIDATION-CONTRACT.md`. They do not
establish installed, behavioral, visual, owner-accepted, publication-ready, or
compatibility status.

Publication requires a separate explicit owner decision after all required
readiness, metadata, licensing, and distribution checks are satisfied. No
workflow credential or Thunderstore upload step is added by inference.
