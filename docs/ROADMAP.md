# Sprint 1 Roadmap - Safe Source Foundation

## Status

**Status:** In progress

**Active story:** S1-04 - Define panel geometry and clamping

**Active story state:** Active; implementation pending

**Parent roadmap:**
[`DSP Recipe Tracker - MVP Roadmap`](planning/MVP-ROADMAP.md)

**Goal:** Establish a reviewable, buildable source foundation without
executing project code in Dyson Sphere Program, modifying the installed game,
or collecting data from the player's runtime environment.

This owner-accepted record is the sanitized replacement for the earlier
"Executable Panel Foundation" draft. It deliberately narrows the parent
roadmap. The earlier draft's installed-game checkpoints, loaded-game
interaction, and retained runtime evidence are not authorized by this record.

## Safety boundary

Sprint 1 is source-only. Work authorized by this roadmap is confined to the
tracked repository and repository-local build output.

### Allowed

- read tracked repository files and the documented external authority inputs;
- read and hash explicitly documented installed assemblies without copying,
  loading, or modifying them;
- inspect managed assembly metadata as a read-only input when an exact API
  shape is required;
- create source, tests, documentation, and build/package output inside the
  repository;
- compile against documented external references without copying them; and
- run deterministic tests that do not start DSP, BepInEx, Unity, Steam, or a
  substitute runtime harness.

### Not authorized

- launching DSP or any executable from the game installation;
- opening or loading a save, entering a game session, or automating game input;
- copying, installing, moving, replacing, or deleting anything under the DSP
  installation, including `BepInEx/plugins`, `BepInEx/config`, and patcher
  directories;
- loading a produced plugin into DSP, BepInEx, Unity, or an isolated harness;
- attaching, injecting, patching, or subscribing to a live game process;
- reading or retaining the installed plugin inventory, player logs,
  configuration, saves, screenshots, paths, or other environment-derived
  evidence for Sprint acceptance;
- copying or redistributing game, Unity, or BepInEx binaries; and
- claiming installed, in-game, visual, interaction, coexistence, cleanup, or
  compatibility validation.

Passing a source build does not weaken this boundary. No agent may convert a
compile result into permission to install or execute the output.

## Owner-performed runtime validation

Runtime validation is outside Sprint 1 and is never performed by an agent or
automation. Later roadmaps postpone human checks until a coherent testable DLL
makes related in-game observations meaningful together. The implementor must
first exhaust non-runtime validation, then provide the owner with the bounded,
self-contained instructions required by `VALIDATION-CONTRACT.md`.

There is no separate controlled-runtime validation plan. The owner alone runs
the supplied DLL and reports the result. Procedures must not assume project
knowledge or expand into broad matrices, repetitive cases, intricate setup, or
evidence collection unrelated to the claim being judged.

## Authority reconciliation

| Source | Sprint 1 consequence |
| --- | --- |
| [`PRODUCT-PRINCIPLES.md`](PRODUCT-PRINCIPLES.md) | Preserve the native-extension design, but do not access live native resources in Sprint 1. |
| [`PROJECT.md`](PROJECT.md) | Governs owner-accepted product behavior and scope. |
| [`IMPLEMENTATION-CONTRACT.md`](IMPLEMENTATION-CONTRACT.md) | Governs architecture, integration mechanics, state ownership, and source constraints. |
| [`VALIDATION-CONTRACT.md`](VALIDATION-CONTRACT.md) | Governs readiness states, evidence, owner acceptance, and publication claims. |
| [`FEASIBILITY.md`](FEASIBILITY.md) | Treat existing findings as historical design evidence; do not repeat its probes or promote them to current runtime validation. |
| [`RUNTIME-AUTHORITY.md`](RUNTIME-AUTHORITY.md) | Records the external source identities used by the recovered design; Sprint 1 does not copy or execute them. |
| [`BEPINEX-CONFORMANCE.md`](BEPINEX-CONFORMANCE.md) | Compile only against the pinned BepInEx 5 surface; do not launch or alter the installed loader. |
| [`THUNDERSTORE-PACKAGE.md`](THUNDERSTORE-PACKAGE.md) | Governs the single-DLL package layout, version mapping, and static package validation. |
| [`planning/MVP-ROADMAP.md`](planning/MVP-ROADMAP.md) | Preserve the cross-sprint product sequence while this Sprint 1 record narrows its execution boundary. |

## Entry gates

The entry gates required to activate S1-01 are satisfied:

- the owner accepted the sanitized safety boundary and linked contract set;
- the product contract is restored and owner accepted;
- repository ignore rules cover dependency binaries, `bin/`, `obj/`, package
  output, and runtime evidence; and
- S1-01 and its exact scope were recorded as Active before implementation.

The owner-approved BepInEx plugin GUID is `dsprecipetracker`, the loader display
name is `DSP-Recipe-Tracker`, and the source/test/CI/package layout is fixed by
`IMPLEMENTATION-CONTRACT.md`. These S1-02 decision gates are satisfied. The
owner accepted S1-01 and activated S1-02 on 2026-08-14.

No implementation agent may infer these decisions from the concept record,
the old roadmap, existing installed plugins, or feasibility artifacts.

## Sprint boundaries

### Included

- repository hygiene and explicit external-reference configuration;
- a minimal `net472` plugin source project with owner-approved identity;
- aligned semantic, assembly, and file versions;
- compile-time BepInEx lifecycle and logging integration;
- UI-independent panel geometry, clamping, and visibility policy;
- compile-time Unity UI boundary code only where supported by confirmed API
  evidence;
- minimal source-defined game and Unity compile-reference shims with complete
  consumed-surface coverage validation for hosted builds;
- deterministic repository-local tests; and
- local and hosted package construction and static package inspection.

### Excluded

- all installed and loaded-game validation;
- Replicator listeners, right-click pinning, and recipe-cell treatment;
- recipe IDs, pin ordering, FILO eviction, and live row population;
- recipe, technology, inventory, crafting, save, or factory access;
- live HUD discovery or native-resource acquisition;
- visual acceptance, screenshots, input simulation, and click-through claims;
- production Hide and global Show/Hide controls;
- tracker navigation, HarmonyX, configuration, persistence, and compatibility
  adapters; and
- hosted access to licensed game or Unity binaries.

## Epic 1 - Repository and build authority

### S1-01 - Establish repository hygiene

**Status:** Complete - technically validated and owner accepted on 2026-08-14

**User story:** As a maintainer, I want generated and licensed material kept
out of Git while source builds use explicit external references.

**Definition of done:**

- Repository ignore rules cover `bin/`, `obj/`, package output, dependency
  binaries, save data, logs, screenshots, and runtime evidence.
- Build configuration references required external assemblies from an
  explicit maintainer-supplied location for local builds and the tracked
  compile-reference shims for hosted builds; neither source nor package output
  contains dependency binaries.
- A missing reference produces an ordinary build failure without fallback
  discovery, installation mutation, network acquisition, or environment
  probing.

**Implementation result:**

- `.gitignore` covers generated build output, packages, dependency binaries,
  saves, logs, screenshots, diagnostics, and runtime evidence.
- `Directory.Build.props` exposes the explicit local `GameRoot` paths and the
  fixed tracked hosted-reference root.
- `Directory.Build.targets` supplies fail-fast MSBuild errors `DRT1001` through
  `DRT1006` without discovery, network, execution, or mutation behavior.
- `ci/compile-references/surface-inventory.json` records the current empty DSP
  and Unity production surface without creating later-story shim projects.

**Technical validation:** Passed on 2026-08-14 with:

```powershell
.\scripts\validate\Validate-S1-01.ps1 -GameRoot '<DSP installation>'
```

The validation confirmed every required ignore class, no prohibited tracked
file, valid explicit Local and Hosted modes, the current hosted inventory, and
the expected fail-fast errors for missing mode, missing `GameRoot`, missing
hosted BepInEx input, nonexistent `GameRoot`, and a missing required assembly.
It also confirmed that the shared
build contract rejects a redirected hosted-reference root and contains no
discovery, network, execution, or mutation task.
No game, loader, save, installation, or runtime process was opened or changed.

**Owner acceptance:** Passed on 2026-08-14. The owner explicitly accepted S1-01
and activated S1-02.

### S1-02 - Establish stable identity and a source build

**Status:** Complete - technically validated and owner accepted on 2026-08-14

**User story:** As a maintainer, I want one stable plugin identity and version
contract before feature code depends on them.

**Definition of done:**

- The owner-approved GUID and display name are recorded in the restored
  product contract before code consumes them.
- `DSPRecipeTracker.sln` contains the single shipping project at
  `src/DSPRecipeTracker/DSPRecipeTracker.csproj` and the deterministic test
  project at `tests/DSPRecipeTracker.Tests/DSPRecipeTracker.Tests.csproj`.
- The shipping project targets `net472` and uses only the documented BepInEx 5
  lifecycle, identity, and logging surface.
- Plugin/package semantic version is `M.m.N`; assembly/file version is
  `M.m.N.0`; diagnostic source labels do not change loader identity.
- Version values have one repository-owned source and are not independently
  hard-coded in multiple locations.
- The repository-local `Release` build completes with zero errors when its
  documented external references are available.
- The hosted `Release` build uses tracked declaration-only game and Unity
  compile-reference shims beneath `ci/compile-references`. A validator compares
  every external type and member used by production code with
  `ci/compile-references/surface-inventory.json` and the shim declarations and
  fails unless both cover the complete consumed set.
- No project assembly is copied, installed, or loaded after the build.

**Implementation result:**

- `DSPRecipeTracker.sln` contains the `net472` shipping project and the
  runtime-independent deterministic test project at their contracted paths.
- `DSPRecipeTrackerPlugin` consumes only `BepInPlugin`, `BaseUnityPlugin`, and
  the plugin-owned logger, using the approved GUID and display name.
- `scripts/build/Build-S1-02.ps1` derives semantic, assembly/file, and
  diagnostic versions from `VERSION`, an explicit build number, and an
  explicit source revision.
- Hosted mode builds a compiler-marked `UnityEngine` reference assembly that
  declares only the `MonoBehaviour` surface required through BepInEx's base
  type. No DSP game declaration is currently consumed or shimmed.
- `CompileReferenceValidator` rejects inventory/shim surface differences and
  any production reference to an unlisted shimmed type or method.

**Technical validation:** Passed on 2026-08-14 with:

```powershell
$revision = git rev-parse HEAD
.\scripts\validate\Validate-S1-02.ps1 `
  -GameRoot '<DSP installation>' `
  -BuildNumber 1 `
  -SourceRevision $revision
```

Both Local and Hosted `Release` builds completed with zero warnings and zero
errors. Deterministic identity/version tests, assembly and file version checks,
diagnostic-label checks, and hosted compile-reference coverage validation
passed. Build outputs remained beneath ignored repository-local paths. No
project or dependency assembly was copied, installed, or loaded after build;
DSP, BepInEx, Unity, Steam, and saves were not executed or opened.

**Owner acceptance:** Passed on 2026-08-14. The owner explicitly accepted S1-02
and activated S1-03.

### S1-03 - Construct and inspect a package

**Status:** Complete - technically validated and owner accepted on 2026-08-14

**User story:** As a maintainer, I want package construction to describe the
real build output without implying that the package has run successfully.

**Definition of done:**

- Local package construction consumes the real `Release` output and writes
  only beneath a repository-local ignored output directory.
- GitHub Actions builds against the validated compile-reference shims and uses
  the same package construction and static validators as the local path.
- Static validation rejects zero-byte, non-managed, version-mismatched, and
  plugin-metadata-mismatched DLLs.
- Static validation rejects any shim or shim-inventory gap and rejects every
  compile-reference shim or dependency binary from package output.
- Package metadata states that installed and in-game validation have not been
  performed and that the artifact is not release-ready.
- No package step writes to the DSP installation or starts a game, loader, or
  harness process.

**Implementation result:**

- `packaging/` contains the tracked manifest template, placeholder package
  README pending owner-approved public copy, and owner-supplied 256x256 PNG
  icon. Generated versions replace the manifest token only in ignored staging
  output.
- `scripts/build/Build-S1-03.ps1` consumes the real S1-02 `Release` DLL and
  creates the exact four-entry ZIP beneath `artifacts/package/`.
- `PackageValidator` inspects the ZIP without loading the plugin. It verifies
  portable exact-case paths, package size, manifest field limits and
  dependency, README encoding, PNG format/dimensions, managed assembly
  identity, all version forms, BepInEx GUID/display/version metadata, and the
  packaged DLL hash. Editorial manifest and README copy is not fixed by the
  validator.
- `Validate-S1-03.ps1` runs both Local and Hosted package paths and confirms
  rejection of zero-byte and non-managed DLLs, version and plugin-metadata
  mismatches, a compile-reference shim, and a dependency binary.
- `.github/workflows/build-package.yml` checks out the triggering commit,
  acquires only the hash-verified documented BepInEx compile reference, runs
  the same hosted build and validators, and retains the ZIP, DLL, build
  information, and validation report for 30 days. Every action is pinned to a
  full reviewed commit SHA; the workflow has read-only repository permission
  and contains no publication step.

**Technical validation:** Passed on 2026-08-14 with:

```powershell
$revision = git rev-parse HEAD
.\scripts\validate\Validate-S1-03.ps1 `
  -GameRoot '<DSP installation>' `
  -BuildNumber 2 `
  -SourceRevision $revision
```

Both Local and Hosted `Release` builds completed with zero warnings and zero
errors. Source identity tests, shim consumed-surface validation, positive
package inspection, and all six required rejection cases passed. The inspected
ZIP contained only `manifest.json`, `README.md`, `icon.png`, and the real
`DSPRecipeTracker.dll` at its contracted path; its DLL hash matched the build
output. A disposable check also confirmed the documented Thunderstore BepInEx
download contains exactly one assembly with the accepted 5.4.17.0 hash.

All generated output remained under ignored repository-local paths. No package
step wrote to the DSP installation or started DSP, BepInEx, Unity, Steam, a
save, or a substitute runtime. Package inspection establishes no installed,
behavioral, visual, compatibility, owner-accepted, publication-ready, or
supported-release claim.

**Owner acceptance:** Passed on 2026-08-14. The owner explicitly accepted
S1-03 and activated S1-04.

## Epic 2 - Testable panel logic

### S1-04 - Define panel geometry and clamping

**Status:** Active - implementation pending

**User story:** As a maintainer, I want deterministic panel geometry before it
is connected to Unity objects.

**Definition of done:**

- UI-independent types represent the panel rectangle, parent bounds, and drag
  delta without referencing live Unity state.
- The implementor records fixed panel dimensions and a concise rationale
  consistent with the three-row product contract.
- A deterministic clamp keeps the full panel rectangle inside a parent that
  can contain it and defines explicit behavior when the parent is smaller.
- Focused tests cover all four edges, corners, repeated drag deltas, and a
  parent-size change.
- Tests run without loading Unity, DSP, BepInEx, or installed assemblies into
  the test process.
- No visual, resolution, scale, or live-drag claim is made from these tests.

### S1-05 - Define the visibility policy

**Status:** Proposed

**User story:** As a maintainer, I want one deterministic visibility rule that
later UI code can consume without owning product state.

**Definition of done:**

- A UI-independent policy computes visibility as
  `hasRows && manualRequested && !majorInterfaceActive`.
- Deterministic tests cover the complete three-input truth table.
- The policy does not inspect DSP, Unity objects, windows, saves, or input.
- Sprint 1 provides no production Hide control, restore control, or
  major-window runtime adapter.

### S1-06 - Preserve the Unity UI boundary without executing it

**Status:** Proposed

**User story:** As a maintainer, I want compile-time UI ownership boundaries
that do not disguise unvalidated runtime behavior as complete.

**Definition of done:**

- Unity-facing code owns only future object creation, layout, raycast, drag,
  and visibility application responsibilities.
- Runtime collection, tracker rules, and presentation values remain outside
  the Unity-facing layer.
- Version-sensitive members are isolated behind a narrow boundary and are
  supported by the accepted recovered feasibility conclusions.
- Missing runtime members are designed to fail softly, but that behavior is
  recorded as unverified until the owner performs an applicable later bounded
  human check.
- Source review confirms there is no startup fixture, automatic launch,
  installation, process attachment, file copy, or environment-data capture.
- Pointer containment, native appearance, cleanup, and live visibility remain
  explicitly unvalidated.

## Sequence and dependencies

```text
Owner approval and restored product contract
    |
S1-01 repository hygiene
    |
S1-02 stable identity and source build
    +--> S1-03 local and hosted package
    +--> S1-04 panel geometry and clamp tests
    +--> S1-05 visibility policy tests
    `--> S1-06 compile-time Unity UI boundary
```

Stories are activated one at a time. Completion of an earlier story does not
implicitly activate or authorize a later story.

## Sprint exit gates

### Source-ready

- Every story explicitly promoted into Sprint 1 meets its definition of done.
- Focused deterministic tests and the local authority-backed `Release` build
  pass from documented repository-local commands.
- The hosted `Release` build and shim consumed-surface coverage validation pass
  in GitHub Actions.
- Static security review confirms that scripts, build targets, tests, and
  package steps cannot write to or execute from the DSP installation.
- Final tracked changes contain no dependency binary, save data, environment
  log, screenshot, generated runtime output, or secret.
- Documentation distinguishes compiled behavior from runtime behavior that
  remains unknown.

### Package-inspected

- Local and hosted packages contain the intended managed build output and
  version-aligned metadata.
- Static validators pass without installing or loading the package.
- The artifact remains marked not release-ready and not runtime-validated.

### Owner-reviewed

- The owner reviews the source evidence, test results, package inspection,
  unknowns, and known limitations.
- Owner review does not assert installed compatibility, visual acceptance, or
  publication readiness.

## Explicitly unavailable states

This sprint cannot reach `Behavioral-ready`, installed, visual, interaction, or
`Publication-ready` status because it produces no meaningful owner runtime
gate. Those states must not be inferred from source readiness, package
inspection, prior feasibility probes, or owner review of this sprint.

## Remaining decisions

Panel dimensions, cell-treatment opacity, and the global Show/Hide icon and
fallback text are implementor-owned choices governed by
`IMPLEMENTATION-CONTRACT.md`. They receive practical non-runtime validation
first and owner human validation only in a later meaningful testable build.

The owner resolves only the product decisions listed in `PROJECT.md`, and only
before their consuming stories become Ready. No runtime-validation planning
decision is required during this sprint.
