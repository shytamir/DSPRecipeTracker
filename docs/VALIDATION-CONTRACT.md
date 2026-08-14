# DSP Recipe Tracker - Validation Contract

## Status and authority

**Status:** Current validation contract

**Implementation status:** Sprint 1 story S1-01 is implemented and technically
validated. It remains Active pending owner acceptance. No functional plugin or
supported release exists.

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
| Behavioral-ready | The owner has completed the applicable bounded human checks through the real integration boundary. |
| Owner-accepted | The owner explicitly accepts the demonstrated result and recorded limitations. |
| Publication-ready | Every required technical, owner, metadata, and distribution gate is explicitly satisfied. |

These states are independent. One state does not imply another.

## 2. Owner-performed runtime validation boundary

All installed and in-game validation is performed by the owner. Agents and
automation may prepare a versioned testable DLL, complete human instructions,
and non-runtime evidence, but may not:

- launch DSP, BepInEx, Unity, Steam, or a substitute runtime harness;
- copy or install a project assembly under the DSP installation;
- open a save or enter a game session;
- automate input or attach to a live game process;
- inspect the player's plugin inventory, configuration, saves, or logs; or
- collect environment-derived logs, screenshots, paths, or other evidence.

Human validation is used only for claims that deterministic tests, static
inspection, compilation, package inspection, or source review cannot establish.
It is postponed until a coherent testable build makes several related
observations meaningful together. There is no separate controlled-runtime
validation program and no requirement for an elaborate environment-isolation,
evidence-retention, or recovery plan.

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

Behavioral readiness becomes available only at a meaningful roadmap gate after
all applicable non-runtime checks pass. The owner runs the supplied testable
DLL and reports the result of the smallest representative procedure needed to
judge the remaining claims.

Human checks normally cover only matters such as:

- whether the real Replicator gesture integrates without breaking native
  selection, hover, disabled, or machine-only presentation;
- whether panel composition, cell-treatment opacity, icon choice, text,
  dragging, input containment, and visibility are clear and usable in-game;
- whether live recipe and inventory values appear and refresh through the real
  integration boundary; and
- whether ordinary load, close, reopen, and shutdown behavior exposes a
  lifecycle or cleanup problem that source-level checks cannot establish.

Ordering, eviction, sufficiency arithmetic, visibility truth tables, package
contents, version alignment, and other deterministic rules remain automated
checks rather than human workload. A human procedure does not repeat them
unless one short observation is necessary to confirm their real integration.
Navigation is included only if the owner has promoted it into active scope.

## 6. Evidence rules

- Use the narrowest check that proves the applicable contract.
- Prefer deterministic, static, build, and package validation over human
  validation whenever either can prove the same claim.
- Record the command or supplied human step set, result, and known limitation
  for every claimed check. Owner-performed validation does not require an
  environment inventory.
- Compilation cannot prove runtime behavior, layout, interaction, cleanup,
  coexistence, or compatibility.
- Historical feasibility probes establish only the facts explicitly recorded
  in `FEASIBILITY.md`; they do not validate a future implementation.
- A skipped or unavailable check is reported as skipped or unavailable, never
  passed.
- Technical evidence does not record owner acceptance by inference.
- Evidence must not include save data, secrets, player diagnostics, unrelated
  plugin identifiers, or unnecessary machine-specific paths.
- Owner-performed human validation may be recorded as a concise pass, fail, or
  qualified result against the supplied steps. Screenshots or logs are requested
  only when they are necessary to decide a specific otherwise-unprovable claim.

## 7. Owner acceptance and publication

- The owner explicitly accepts or rejects a demonstrated result after
  reviewing its evidence and limitations.
- Owner review of source or a package does not imply behavioral acceptance.
- Behavioral readiness does not imply publication approval.
- Publication requires truthful public documentation, version-aligned package
  metadata, required distribution checks, and explicit owner authorization.
- No package is described as supported, release-ready, or publishable before
  every required state is explicitly satisfied.

## 8. Human validation procedure contract

Every owner-performed procedure must:

- identify the exact test DLL or package version and the purpose of the gate;
- assume no knowledge of the project, its architecture, or earlier discussions;
- give complete prerequisites, setup, actions, expected observations, and a
  simple way to report pass, fail, or an unexpected result;
- group related observations into one meaningful session and avoid isolated
  cosmetic checkpoints, broad matrices, repetitive cases, or intricate setup;
- include only safety, cleanup, or diagnostic steps directly necessary for the
  bounded test; and
- distinguish what the owner observed from what automated validation already
  proved.

The implementor proposes the procedure with the testable build. The owner alone
runs it and decides whether the human gate passes, needs repair, or is deferred.

## 9. Validation change control

Changing an acceptance criterion requires product review when it weakens or
changes a behavior in `PROJECT.md`. Adding human workload requires evidence
that the claim cannot be established reasonably by non-runtime validation. A
roadmap status change, successful command, owner-run observation, or prior probe
never supplies owner acceptance or publication approval by inference.
