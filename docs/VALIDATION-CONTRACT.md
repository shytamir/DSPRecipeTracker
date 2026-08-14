# DSP Recipe Tracker - Validation Contract

## Status and authority

**Status:** Current validation contract

**Implementation status:** Implementation has started. Sprint 1 story S1-01 is
Active and pending implementation. No functional plugin or supported release
exists.

**Owner review:** Accepted on 2026-08-14.

This file governs readiness states, validation evidence, runtime authorization,
owner acceptance, and publication claims. Product behavior comes from
[`PROJECT.md`](PROJECT.md); implementation obligations come from
[`IMPLEMENTATION-CONTRACT.md`](IMPLEMENTATION-CONTRACT.md); active scope and
operational authorization come from [`ROADMAP.md`](ROADMAP.md).

This contract does not authorize runtime execution or mark any readiness state
as achieved.

## 1. Readiness states

| State | Meaning |
| --- | --- |
| Source-ready | The implemented source scope builds and its deterministic checks pass. |
| Package-inspected | A real package contains the intended version-aligned build output and passes static inspection. |
| Behavioral-ready | Separately authorized evidence demonstrates applicable behavior through the real integration boundary. |
| Owner-accepted | The owner explicitly accepts the demonstrated result and recorded limitations. |
| Publication-ready | Every required technical, owner, metadata, and distribution gate is explicitly satisfied. |

These states are independent. One state does not imply another.

## 2. Current runtime authorization boundary

The current Sprint 1 roadmap is source-only. Unless a later user prompt
explicitly authorizes an owner-reviewed controlled-runtime procedure, no agent
may:

- launch DSP, BepInEx, Unity, Steam, or a substitute runtime harness;
- copy or install a project assembly under the DSP installation;
- open a save or enter a game session;
- automate input or attach to a live game process;
- inspect the player's plugin inventory, configuration, saves, or logs; or
- collect environment-derived logs, screenshots, paths, or other evidence.

A controlled-runtime procedure must define its target environment, isolation,
permitted reads and writes, evidence handling, backup and recovery, cleanup,
and stop conditions before execution.

Until such a procedure is approved, runtime, installed, visual, interaction,
cleanup, coexistence, and compatibility behavior remains unvalidated. That is
an unavailable state, not blocked work to work around.

## 3. Source-ready contract

Source readiness requires, for the implemented and active roadmap scope:

- a `Release` build with zero errors against the documented external
  references;
- focused deterministic tests for product rules that can be exercised without
  DSP or Unity;
- hosted compile-reference shims contain declarations only, are excluded from
  package output, and pass the complete consumed-surface coverage validation
  defined by `THUNDERSTORE-PACKAGE.md`;
- no dependency binary, save, player diagnostic, secret, or generated runtime
  evidence in tracked changes; and
- documentation that distinguishes compiled behavior from unvalidated runtime
  behavior.

As applicable stories are implemented, deterministic coverage includes:

- pin toggle, order, capacity, and FILO eviction;
- direct-ingredient sufficiency at below, equal, and above the requirement;
- invalid-recipe removal while preserving remaining order;
- panel geometry and clamping edge cases; and
- the complete `hasRows`, `manualRequested`, and `majorInterfaceActive` truth
  table.

Source readiness does not establish installed or in-game behavior.
Compilation against a validated shim establishes only source compatibility
with its declared surface; it does not establish runtime signature fidelity,
behavior, or compatibility.

## 4. Package-inspected contract

Package inspection requires:

- construction from the real version-aligned `Release` output;
- output only beneath an ignored repository-local artifact directory;
- rejection of zero-byte, non-managed, version-mismatched, and plugin-
  metadata-mismatched assemblies;
- no copied game, Unity, or BepInEx binary;
- no save, log, screenshot, secret, or player-specific path; and
- metadata that states installed and in-game validation truthfully.

Static inspection does not install or load the package and does not establish
behavioral, owner-accepted, or publication-ready status.

## 5. Behavioral-ready contract

Behavioral readiness is unavailable under the current source-only roadmap. If
a controlled-runtime procedure is separately authorized, its evidence must
exercise the real applicable integration boundary and demonstrate:

### 5.1 Pinning and state

- left-click selects without toggling a pin;
- right-click selects and toggles only an eligible native recipe cell;
- toggling the same recipe off preserves other entries;
- a fourth pin evicts the bottom entry and preserves remaining order; and
- crafting does not remove a pin.

### 5.2 Material presentation

- inventory changes refresh every displayed numerator;
- sufficiency changes at the exact required threshold;
- only direct ingredients are displayed; and
- machine-only rows name the required facility without recursive ingredient
  warnings.

### 5.3 Native integration and panel behavior

- native hover, selection, disabled, and machine-only states remain intact;
- tracker cell treatment does not change DSP's original grid-state buffer;
- the panel stays reachable after dragging and parent-size changes;
- panel interaction does not activate the world or a covered HUD control;
- the exact six-interface visibility rule is applied;
- panel Hide and global Show/Hide preserve the player's manual choice; and
- the normal empty state remains hidden.

### 5.4 Failure and lifecycle

- missing runtime evidence fails softly without blocking the Replicator or
  corrupting remaining pin order;
- diagnostics remain bounded; and
- shutdown releases tracker-owned subscriptions and resources.

Navigation evidence applies only if the owner explicitly promotes navigation
into the active product and roadmap scope.

## 6. Evidence rules

- Use the narrowest check that proves the applicable contract.
- Record the command, environment boundary, result, and known limitation for
  every claimed check.
- Compilation cannot prove runtime behavior, layout, interaction, cleanup,
  coexistence, or compatibility.
- Historical feasibility probes establish only the facts explicitly recorded
  in `FEASIBILITY.md`; they do not validate a future implementation.
- A skipped or unavailable check is reported as skipped or unavailable, never
  passed.
- Technical evidence does not record owner acceptance by inference.
- Evidence must not include save data, secrets, player diagnostics, unrelated
  plugin identifiers, or unnecessary machine-specific paths.

## 7. Owner acceptance and publication

- The owner explicitly accepts or rejects a demonstrated result after
  reviewing its evidence and limitations.
- Owner review of source or a package does not imply behavioral acceptance.
- Behavioral readiness does not imply publication approval.
- Publication requires truthful public documentation, version-aligned package
  metadata, required distribution checks, and explicit owner authorization.
- No package is described as supported, release-ready, or publishable before
  every required state is explicitly satisfied.

## 8. Open validation decision

No controlled-runtime procedure is approved. Its target environment,
isolation, permitted reads and writes, evidence set, privacy boundary, cleanup,
recovery, and stop conditions remain an explicit future owner decision.

## 9. Validation change control

Changing an acceptance criterion requires product review when it weakens or
changes a behavior in `PROJECT.md`. Changing runtime evidence collection
requires a separately owner-approved operational procedure. A roadmap status
change, successful command, or prior probe never supplies either approval by
inference.
