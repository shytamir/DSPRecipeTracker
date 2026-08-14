# DSP Recipe Tracker - Thunderstore Package Contract

## Status and authority

**Status:** Current package contract

**Implementation status:** The package pipeline is pending implementation in
Sprint 1. No installable package or supported release exists.

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

Sprint 1 implements local package construction and the same static validators
that future automation will use. Hosted executable packaging remains disabled
until an authority record establishes a lawful, complete game and Unity
reference source. Once that gate is satisfied, a workflow triggered by each
push to `main` and by manual dispatch will:

1. verify that it checked out the triggering commit;
2. derive all version forms from `VERSION`, the run number, and commit;
3. build the real `net472` plugin and run the implemented deterministic tests;
4. validate that `DSPRecipeTracker.dll` is a non-empty managed assembly with
   the expected name, semantic metadata, assembly version, file version, and
   BepInEx plugin identity;
5. build the Thunderstore ZIP from real build output and tracked package
   assets;
6. validate the exact archive allowlist, portable names, root files, manifest
   shape, semantic version, README presence, PNG format and dimensions, DLL
   path, and recorded DLL hash; and
7. upload the ZIP, recipe-tracker DLL, build information, and static validation
   reports as one workflow artifact retained for 30 days.

Until the hosted gate is satisfied, CI runs only checks that require no game or
Unity binaries and emits no DLL or mod ZIP. Local package construction receives
an explicit non-negative patch value corresponding to `N`; authorized GitHub
automation supplies its run number. Neither path edits `VERSION`.

The workflow does not publish to Thunderstore, create a GitHub release, or
describe the artifact as supported or release-ready.

Generic package validation checks structure, identity, integrity, and version
consistency. It does not claim that recipe tracking, UI, interaction, runtime
cleanup, compatibility, or any other product behavior works.

## 4. Build prerequisites and reference boundary

- Local builds use an explicit `GameRoot` setting that points to the supported
  DSP installation. Game, Unity, and BepInEx assemblies are read as compile
  inputs and are never copied into source control or package output.
- Hosted CI may acquire the exact documented BepInEx `5.4.17` package when the
  consumed assembly identity matches the conformance contract.
- Hosted CI must not download, upload, cache, reconstruct, or redistribute
  licensed DSP or Unity binaries.
- Partial DSP API shims, extracted implementation, and source declarations
  that pretend to be the supported game assembly are prohibited.
- If complete lawful game and Unity compile references are unavailable on the
  hosted runner, executable building is reported as unavailable and no mod ZIP
  or DLL artifact is emitted. Source and documentation checks that make no
  executable claim may still run.
- The implemented automation uses the repository-selected .NET SDK and
  PowerShell tooling recorded by the active Sprint 1 story.

## 5. Package assets

- `manifest.json` uses the generated semantic version and the package identity
  approved for DSP Recipe Tracker. Its runtime dependency is the supported
  Thunderstore BepInEx package for the fixed `5.4.17` loader line documented in
  `BEPINEX-CONFORMANCE.md`.
- `README.md` describes only behavior and readiness states supported by the
  current product and validation contracts.
- `icon.png` satisfies Thunderstore's required PNG format and dimensions and
  contains no copied game art.
- Package assets are tracked source inputs; generated archives and binaries
  remain ignored build output.

## 6. Validation and publication boundary

Package construction and static inspection may establish only
`Package-inspected` status as defined by `VALIDATION-CONTRACT.md`. They do not
establish installed, behavioral, visual, owner-accepted, publication-ready, or
compatibility status.

Publication requires a separate explicit owner decision after all required
readiness, metadata, licensing, and distribution checks are satisfied. No
workflow credential or Thunderstore upload step is added by inference.
