# Sprint 1 Roadmap - Safe Source Foundation

## Status

**Status:** In progress

**Active story:** S1-01 - Establish repository hygiene

**Active story state:** Active and pending implementation

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

## Separate future authorization

Runtime validation is outside Sprint 1. It may be proposed later only as a
separate owner-approved record that defines, before any execution:

- the exact target environment and permitted process;
- isolation from the player's normal plugins, configuration, saves, and logs;
- permitted filesystem reads and writes;
- backup, cleanup, and recovery procedures;
- the precise evidence to retain and its privacy review; and
- stop conditions for unexpected plugins, data, prompts, errors, or writes.

Until that record is approved, runtime validation is **not blocked work to work
around**. It is work outside the authorized scope.

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
- S1-01 and its exact scope are recorded as Active below.

The owner-approved BepInEx plugin GUID is `dsprecipetracker`, the loader display
name is `DSP-Recipe-Tracker`, and the source/test/CI/package layout is fixed by
`IMPLEMENTATION-CONTRACT.md`. These S1-02 decision gates are satisfied, but
S1-02 remains Proposed until S1-01 is completed and the owner activates it.

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

**Status:** Active - pending implementation

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

### S1-02 - Establish stable identity and a source build

**Status:** Proposed

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

### S1-03 - Construct and inspect a package

**Status:** Proposed

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

## Epic 2 - Testable panel logic

### S1-04 - Define panel geometry and clamping

**Status:** Proposed

**User story:** As a maintainer, I want deterministic panel geometry before it
is connected to Unity objects.

**Definition of done:**

- UI-independent types represent the panel rectangle, parent bounds, and drag
  delta without referencing live Unity state.
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
  recorded as unverified until separately authorized runtime validation.
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

This sprint cannot reach `Installed-ready`, `Runtime-validated`,
`Visually-validated`, or `Publication-ready`. Those states require separate
authorization and evidence. They must not be inferred from source readiness,
package inspection, prior feasibility probes, or owner review of this sprint.

## Remaining owner decisions

- Decide the exact fixed panel dimensions and calibrated cell-treatment
  opacity before S1-04 becomes Ready.
- Decide the global Show/Hide icon and tracker-owned fallback text before a
  story implements that production control.
- Resolve the product decisions listed in `PROJECT.md` only before their
  consuming stories become Ready.
- Decide later whether to commission a separate controlled-runtime validation
  plan. That decision is not required to begin safe source work.
