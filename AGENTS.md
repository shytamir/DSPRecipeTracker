# AGENTS.md

This file defines how coding agents should work in this repository.

## 1. Core rule

Complete the requested task with the smallest coherent change that satisfies
its acceptance criteria.

Do not turn bounded work into a repository-wide review. Optimize for
correctness, maintainability, and reviewability rather than activity.

## 2. Instruction order

Follow, in order:

1. The current user prompt.
2. This `AGENTS.md`.
3. Repository documentation and conventions.
4. Existing implementation patterns.
5. General engineering judgment.

A specific instruction overrides a general one.

## 3. Product contract

DSP Recipe Tracker is a BepInEx-dependent Dyson Sphere Program mod that lets
players pin recipes from the Replicator and compare their direct material
requirements with the contents of Icarus's inventory.

Preserve these invariants unless the task explicitly changes one:

- the player explicitly owns pinning, unpinning, and tracker visibility;
- the native Replicator recipe grid is the only pinning surface, and no
  separate Pin/Unpin button is added;
- left-click remains native selection-only, while right-click follows native
  selection and toggles the pin through the existing recipe-grid event;
- tracker cell treatment uses an independent non-raycasting clone and state
  buffer; do not write pin state into DSP's original recipe-grid buffer;
- the tracker reads inventory and recipe state but does not alter inventory,
  crafting, the factory, or save data;
- the MVP tracks Icarus inventory only and direct ingredients only;
- only unlocked recipes may be pinned;
- at most three recipes are pinned, and a fourth replaces the bottom entry;
- crafting does not remove a pin;
- tracker product and ingredient representations remain non-interactive in the
  MVP; tracker navigation is out of scope;
- unavailable or renamed runtime evidence fails softly;
- runtime collection, tracker state, presentation modeling, and Unity UI stay
  separate;
- quantity scaling, saved pins, automatic replication, animation, and panel
  resizing remain outside the MVP until explicitly promoted.

Read `docs/PROJECT.md` before changing product behavior, runtime integration,
state handling, interaction, or UI.

Read `docs/IMPLEMENTATION-CONTRACT.md` before changing implementation and
`docs/VALIDATION-CONTRACT.md` before making readiness, acceptance,
compatibility, release, or publication claims.

`docs/PRODUCT-PRINCIPLES.md` defines the native-extension approach, and
`docs/FEASIBILITY.md` records the confirmed integration surface and remaining
runtime checkpoints. Follow both when implementing Replicator or tracker UI
behavior; do not turn feasibility recommendations into broader game rules.

`docs/planning/PINNED-RECIPE-TRACKER-CONCEPT.md` preserves the original,
non-authoritative planning foundation. Use it for intent, design history,
deferred ideas, and open questions; `docs/PROJECT.md` governs whenever they
differ.

`docs/ROADMAP.md` is the current planning and implementation-authorization
record. Read it before changing implementation or management state. When it
marks no story Active, implementation is not authorized. Otherwise, work only
the story marked Active. Do not mark technical validation or owner acceptance
by inference.

## 4. Scope

Inspect and modify only:

- files named in the task;
- files directly required to implement it;
- directly affected validation code;
- directly affected documentation.

Do not fix unrelated defects, modernize nearby code, upgrade unrelated
dependencies, or reorganize files without necessity. Mention relevant
unrelated findings in the final report, but do not fix them unless they block
the requested task.

## 5. Before editing

1. Run `git status --short`.
2. Inspect directly relevant source and documentation.
3. Identify existing behavior and local conventions.
4. Determine the smallest viable implementation.
5. Identify the narrowest relevant validation.
6. Start editing once the task is sufficiently understood.

Resolve minor ambiguity from repository evidence. Ask only when a missing
decision would materially change the outcome.

## 6. Mutating and non-mutating work

Treat requests to inspect, review, analyze, explain, or plan as non-mutating
unless they explicitly ask for changes.

Unless the prompt says `PLAN ONLY`, a request to implement, author, update, or
publish is implementation work:

1. inspect;
2. implement;
3. validate;
4. repair failures caused by the change;
5. review the final diff;
6. commit only when requested;
7. push only when explicitly requested;
8. report after required Git operations succeed.

## 7. Planned architecture

Keep these responsibilities separate as the project develops:

```text
DSP and BepInEx runtime
    |
Recipe and inventory adapters
    |
Normalized tracker state
    |
Presentation model
    |
Replicator integration and tracker UI
```

- Runtime adapters read game state and isolate version-sensitive access.
- Tracker state owns pin order, the three-recipe limit, and material
  sufficiency.
- The presentation model produces UI-ready values without Unity hierarchy
  manipulation.
- Unity UI code owns layout, input capture, visibility, dragging, and native
  visual integration only.

Do not bury product rules in reflection code or runtime collection in UI code.

## 8. Toolchain and game dependencies

The intended baseline is:

- C# compatible with the selected BepInEx 5 runtime;
- .NET Framework 4.7.2 (`net472`) unless implementation evidence requires a
  different target;
- BepInEx 5;
- Unity and game assemblies supplied by the installed Dyson Sphere Program for
  local builds; and
- minimal source-defined compile-reference shims for hosted builds, with
  complete consumed-surface coverage validation.

Do not copy or commit game, Unity, or BepInEx binaries. Resolve them from the
local game installation, the documented BepInEx CI source, or the tracked
compile-reference shims. Shims must contain declarations only, must cover every
known external symbol consumed by production code, and must never be packaged,
loaded, or treated as runtime evidence.

When authoritative runtime knowledge is required, inspect the installed game
assemblies or actual runtime evidence as read-only inputs. Distinguish
confirmed members from inference, isolate fragile integration points, and
record uncertainty instead of presenting a proxy as fact.

`docs/RUNTIME-AUTHORITY.md` records the recovered authority hierarchy, hashes,
accepted conclusions, and refresh rules. The former retained evidence was
removed and must not be claimed as present. Treat the recorded conclusions as
the accepted recovery baseline. If new runtime claims are required against a
different installed assembly, establish new evidence without mixing it into
the accepted historical snapshot.

`docs/BEPINEX-CONFORMANCE.md` fixes the supported plugin-manager contract at
the installed BepInEx `5.4.17.0` and its pinned `v5.4.17` source tag. Use only
the minimal documented feature surface. Do not target the source checkout's
BepInEx 6 `master`, alter the loader, or add multi-version compatibility by
inference. Reference HarmonyX only when a confirmed hook requires it.

`docs/THUNDERSTORE-PACKAGE.md` defines the package layout, version mapping,
external-reference boundary, static package validation, and publication
boundary. Read it before changing package assets, versioning, build workflow,
artifact retention, or distribution behavior.

## 9. Implementation discipline

Prefer minimal local patches, direct readable code, deterministic state
transitions, and focused validation. Avoid speculative abstractions, broad
refactors, unnecessary compatibility layers, frame-by-frame allocations, and
comments that merely restate code.

Preserve public behavior and stored-data contracts unless the task explicitly
changes them. Do not add persistence, automatic crafting, recursive ingredient
expansion, or broader inventory scopes as incidental enhancements.

## 10. Runtime and UI safety

- Treat DSP and save state as read-only.
- Do not invoke crafting or move items as part of tracking.
- Patch the narrowest stable game surface available.
- Unpatch and release event subscriptions during plugin shutdown.
- Avoid repeated broad reflection scans in update loops.
- Keep the tracker within screen bounds and prevent its controls from passing
  clicks through to the game.
- Hide or defer interaction while incompatible major game interfaces are
  open.
- Fail softly when expected game objects or members are unavailable.

## 11. Validation

Run the narrowest relevant check first:

1. build the affected project;
2. run focused deterministic tests where practical;
3. fix failures caused by the change;
4. rerun the failed check;
5. run broader validation only when justified;
6. review the final diff once.

A release build must complete with zero errors. Compilation alone does not
prove in-game behavior, interaction, layout, performance, or compatibility.
Agents and automation never perform installed or in-game validation. Exhaust
deterministic, static, build, and package checks first. When a remaining claim
can only be judged in DSP, prepare a testable build DLL and a concise human
procedure for the owner at a meaningful roadmap gate. The procedure must assume
no project knowledge, give complete setup, action, expected-result, and result-
reporting instructions, and contain only the smallest practical set of checks.
Do not launch DSP, install a plugin, open a save, automate input, or collect
environment-derived logs or screenshots on the owner's behalf.

If a required tool or game state is unavailable, report the check as skipped
or blocked rather than passed.

When polling GitHub Actions, make at most four total status checks, waiting 10,
20, and 40 seconds before checks two through four. Do not use tight-loop
watchers; if the run remains pending or becomes rate-limited, report that state
instead of continuing to poll.

## 12. Tests and documentation

Add or update tests when behavior changes and a focused deterministic test is
practical. Do not add broad test infrastructure for a small change.

Documentation must match actual behavior. Update `README.md` and
`docs/PROJECT.md` when the public behavior or product contract changes.

Do not commit:

- `bin/` or `obj/`;
- DLLs, PDBs, or copied dependency assemblies;
- anything under `artifacts/`, including retained game assemblies and derived
  runtime datasets;
- save files or diagnostics containing player data;
- temporary files, editor caches, or OS noise.

## 13. Git discipline

Do not overwrite, revert, reformat, or include unexplained user changes.

Before committing:

1. inspect `git status --short`;
2. inspect the final diff;
3. confirm only intended files changed;
4. run required validation;
5. check for secrets, save data, temporary files, and generated output.

Create one coherent commit per requested task unless instructed otherwise. Do
not amend, rebase, reset, clean, stash, force-push, or rewrite history unless
explicitly instructed. Push only when explicitly requested.

If Git reports dubious ownership on Windows, scope the exception to the
current command instead of altering global configuration:

```powershell
$repo = (Resolve-Path '.').Path.Replace('\', '/')
git -c "safe.directory=$repo" status --short
```

Before editing or pushing, fetch or use `git pull --ff-only` when the clean
local branch may be behind its remote. Never resolve divergence with a force
push or history rewrite.

## 14. Stop conditions

Stop and report when:

- completion requires changes outside the authorized scope;
- the task conflicts with the product contract or repository instructions;
- required credentials, dependencies, assemblies, or game evidence are
  unavailable;
- user changes prevent safe modification;
- validation reveals an unrelated repository-wide failure;
- the outcome requires a major product decision not covered by the prompt;
- committing or pushing would include unrelated work.

## 15. Definition of done

A task is complete when the requested artifact or behavior exists, the change
stays within scope, relevant checks pass or skips are accurately reported,
the final diff contains only intentional changes, and requested Git operations
succeed.

## 16. Final report

Report:

### Completed

A concise description of the result.

### Changed

- files created, modified, or removed;
- significant behavioral changes.

### Validation

List each command actually run and its result.

### Git

- branch;
- commit hash and message, when committed;
- push result: successful, failed, or not requested.

### Residual issues

List only known limitations, blockers, or relevant follow-up deliberately left
out of scope. If none, say:

`None known within the requested scope.`
