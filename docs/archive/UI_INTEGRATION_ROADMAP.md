# Sprint 2 Roadmap - Native Integration and Tracker State

## Status and authority

**Status:** Complete - owner accepted on 2026-08-15

**Active story:** None - S2-01 through S2-06 are owner accepted

**Active story state:** Sprint 2 complete

**Implementation authorization:** None - archived historical record

**Parent roadmap:**
[`DSP Recipe Tracker - MVP Roadmap`](../planning/MVP-ROADMAP.md)

**Previous roadmap:**
[`Bootstrap Roadmap - Safe Source Foundation`](BOOTSTRAP-ROADMAP.md)

The owner accepted S2-06 and the Sprint 2 exit on 2026-08-15. All Sprint 2
stories and exit gates are complete and owner accepted. This record is
historical and authorizes no further implementation.

**Goal:** Connect the existing panel boundary to the native Replicator and HUD
surfaces while keeping pin state deterministic, transient, and separate from
runtime adapters and Unity presentation.

**Exit result:** Right-clicking an eligible recipe through the native
Replicator event path drives an ordered three-entry tracker and an independent
green/red recipe-grid treatment. The panel applies the exact six-interface
visibility rule and provides paired panel Hide and native game-menu Show/Hide
controls. Rows show only each pinned recipe's native icon; live ingredient and
inventory presentation remains Sprint 3 work.

The exit result describes intended behavior, not a current runtime claim.

## Safety and validation boundary

Agents and automation perform source, deterministic-test, build, shim-coverage,
static inspection, and package validation only. They do not install or load the
plugin, launch DSP, open a save, automate game input, collect player-environment
evidence, or substitute a harness for the game.

Implementation may use installed assemblies only as read-only authority inputs
when exact signatures, inheritance, or call sites are required. Before relying
on a new runtime claim, the implementor must reconcile it with
[`RUNTIME-AUTHORITY.md`](../RUNTIME-AUTHORITY.md), record the authority used, and
expand the declaration-only hosted shims and consumed-surface inventory to
cover every external production reference. Game, Unity, and BepInEx binaries
must not be copied into source control or package output.

Human checks are postponed until a later meaningful gate combines the live
gesture, native treatment, panel composition, controls, visibility, and Sprint
3 row content in one coherent testable DLL. The eventual procedure is written
for an owner with no assumed project knowledge and contains only the smallest
set of observations that cannot be established another way.

## Entry gates

The entry gates required to activate S2-06 are satisfied:

- the owner accepted this roadmap;
- every decision required by S2-06 is recorded;
- the repository begins from the accepted bootstrap state; and
- S2-01 through S2-05 are owner accepted; and
- exactly S2-06 is marked Active.

Later stories become Active one at a time only after the preceding dependency
is technically validated and explicitly owner accepted. Completion never
activates the next story by inference.

## Sprint boundaries

### Included

- deterministic transient pin state using integer runtime recipe identities;
- native Replicator right-click subscription through the existing shared
  recipe-grid event;
- independent tracker-owned recipe-grid treatment using native resources;
- narrow recipe lookup for three panel slots using each pinned recipe's native
  `RecipeProto.iconSprite`;
- exact Tech, Dyson Editor, Inventory, Replicator, Statistics, and Dashboard
  active-state collection;
- panel Hide and native game-menu Show/Hide controls;
- startup orchestration, bounded diagnostics, and complete owned-resource
  cleanup for the new integration; and
- local and hosted source/package validation with complete shim coverage.

### Excluded

- direct-ingredient and inventory collection or sufficiency presentation;
- machine-only warning presentation beyond preserving native eligibility and
  presentation state;
- tracker-icon navigation;
- persistence, configuration-backed pin state, quantity scaling, recursive
  ingredients, automatic crafting or replication, animation, and resizing;
- broader major-window inference or additional pinning surfaces;
- HarmonyX unless new authoritative evidence proves the accepted native event
  path insufficient and the owner approves a roadmap change;
- installed or in-game execution by agents or automation; and
- release, publication, supported-runtime, or compatibility claims.

## Epic 1 - Deterministic tracker state

### S2-01 - Implement transient pin ordering and capacity

**Status:** Complete - owner accepted on 2026-08-14

**User story:** As a player, I want my explicit recipe pins to occupy a stable
three-entry order so the tracker responds predictably without touching game or
save state.

**Definition of done:**

- A UI-independent tracker-state component stores integer runtime recipe IDs
  and owns all toggle, order, capacity, and removal transitions.
- Toggling an unpinned recipe inserts it at the top while preserving the
  relative order of existing entries.
- Toggling a pinned recipe explicitly unpins it and preserves the relative
  order of the remaining entries.
- A fourth distinct pin evicts the third, bottom entry.
- Safe invalid-recipe removal preserves the remaining order; crafting or
  presentation refresh cannot remove or reorder a pin.
- Pins remain session-only and are never written to a save or BepInEx
  configuration.
- Focused deterministic tests cover empty state, toggle on/off, duplicate
  prevention, all insertion positions, three-entry capacity, fourth-pin FILO
  eviction, and invalid removal.
- Each completed pin-state transition reports one concise Debug diagnostic
  through a UI-independent diagnostic boundary, with the action, affected
  recipe ID, and resulting ordered recipe IDs. The message contains at most the
  three retained IDs and does not dump recipe contents, runtime objects, or
  broader game state. Focused tests assert the Debug level, required fields,
  bounded payload, and absence of a message for a no-op transition.
- Tests run without loading DSP, Unity, BepInEx, the shipping plugin, or a
  substitute runtime.

**Acceptance gate:** Focused tests and Local/Hosted Release builds pass with
zero errors. Source inspection confirms that the component has no runtime,
Unity, persistence, inventory, crafting, or factory dependency. Owner
acceptance is recorded separately.

**Implementation result:**

- `PinnedRecipeState` owns a private ordered list of at most three integer
  recipe IDs and exposes it through a read-only view.
- `Toggle` inserts a new recipe at the top, unpins an existing recipe, and
  reports the bottom eviction when a fourth distinct recipe is pinned.
- `RemoveUnavailable` removes only an existing invalid identity, preserves the
  relative order of remaining pins, and is a silent no-op when the identity is
  absent.
- `PinStateChange` reports the transition kind, affected identity, and optional
  eviction without coupling state to presentation or runtime integration.
- The UI-independent `ITrackerDiagnosticSink` receives one bounded Debug
  message per completed transition with action, affected ID, retained order,
  and optional eviction. No-op removal emits nothing.

**Technical validation:** Passed on 2026-08-14 with:

```powershell
.\scripts\validate\Validate-S2-01.ps1

$revision = git rev-parse HEAD
.\scripts\build\Build-S1-02.ps1 `
  -ReferenceMode Local `
  -GameRoot '<DSP installation>' `
  -BuildNumber 7 `
  -SourceRevision $revision

.\scripts\build\Build-S1-02.ps1 `
  -ReferenceMode Hosted `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 7 `
  -SourceRevision $revision
```

The focused deterministic gate covered empty state, successive top insertion,
explicit unpinning, duplicate prevention, three-entry capacity, fourth-pin
bottom eviction, unavailable middle removal, remaining-order preservation,
transition results, Debug fields and level, bounded message length, eviction
identity, and no-op diagnostic silence. The test process linked the state source
directly and loaded no shipping, Unity, DSP, BepInEx, or substitute-runtime
assembly.

Local and Hosted Release builds completed with zero warnings and zero errors.
The hosted product passed complete compile-reference surface validation. Static
inspection found no Unity, DSP, BepInEx, inventory, crafting, factory,
persistence, or file-system dependency in the state component, and no generated
or binary output entered the tracked tree. No game, loader, save, or runtime was
started or changed.

**Owner acceptance:** Accepted explicitly on 2026-08-14. This acceptance
authorized S2-02; it does not infer acceptance of later work.

## Epic 2 - Native Replicator integration

### S2-02 - Add the native right-click pin gesture

**Status:** Complete - owner accepted on 2026-08-14

**User story:** As a player, I want right-click on an eligible native
Replicator recipe cell to toggle its pin while ordinary recipe selection keeps
working as designed.

**Definition of done:**

- The integration subscribes after Replicator creation to the existing shared
  recipe-grid `PointerDown` event and removes only its own listener during
  shutdown.
- DSP's native selection listener remains in place and runs before tracker
  behavior.
- Tracker behavior filters exclusively for
  `PointerEventData.InputButton.Right`; left and middle clicks never enter
  tracker state.
- The recipe identity comes from the same current grid index and populated
  recipe array used by the native path. Native population remains the
  eligibility boundary.
- Version-sensitive event, index, and array access is isolated behind one
  narrow adapter, resolved outside update loops, and fails softly.
- No Pin/Unpin button, second pinning surface, Harmony patch, parallel unlock
  rule, or crafting behavior is introduced.
- Deterministic tests cover button filtering, native-before-tracker ordering,
  valid and invalid indices, failed binding, and one-time listener cleanup.
- Debug diagnostics record attachment/removal and each accepted right-click
  with grid index, recipe ID, and toggle action. Ordinary rejected buttons are
  silent; failures are logged once per identity. Tests assert those bounds.
- Exact consumed signatures are authority-backed and fully represented by the
  hosted shims and surface inventory.

**Acceptance gate:** Focused tests, Local/Hosted Release builds, exhaustive
shim validation, and static review pass. Live gesture behavior remains
unvalidated pending the later owner gate.

**Implementation result:**

- `ReplicatorPinInput` filters the UI-independent input boundary to right-click,
  resolves the current native recipe identity, and delegates the toggle to
  `PinnedRecipeState`.
- `UnityReplicatorPinInputAdapter` resolves `evtRecipe`, `recipeProtoArray`, and
  `mouseRecipeIndex` once, appends one listener to DSP's existing
  `PointerDown` callback, and removes only that listener during release.
- The existing native callback remains first. Recipe eligibility is inherited
  from the same populated array and current index used by DSP.
- Missing fields, events, indices, or populated recipes disable the boundary
  softly. No Harmony patch, alternate pin surface, unlock model, crafting, or
  persistence behavior was introduced.
- Bounded Debug diagnostics record attach/detach and accepted right-clicks with
  grid index, recipe ID, and action; rejected buttons are silent and failures
  are reported once.

**Technical validation:** Passed on 2026-08-14 with:

```powershell
.\scripts\validate\Validate-S2-02.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>'

$revision = git rev-parse HEAD
.\scripts\build\Build-S1-02.ps1 `
  -ReferenceMode Local `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 8 `
  -SourceRevision $revision

.\scripts\build\Build-S1-02.ps1 `
  -ReferenceMode Hosted `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 8 `
  -SourceRevision $revision
```

Focused tests covered left/middle filtering, native-before-tracker ordering,
pin/unpin handoff, negative and out-of-range indices, unpopulated entries,
failed attachment, inert post-release behavior, one-time cleanup, diagnostic
fields, and rejection silence. Read-only Mono.Cecil inspection rechecked the
recorded Assembly-CSharp and Unity hashes and confirmed the three private
bindings, native handler signature, native index/array use, selection call,
and existing PointerDown callback construction. Local and Hosted Release
builds completed with zero warnings and zero errors; the hosted product passed
the exhaustive declaration-only shim and consumed-surface validator. No game,
loader, save, plugin, or substitute runtime was started or changed.

**Owner acceptance:** Accepted explicitly on 2026-08-14. This acceptance
authorized S2-03; it does not infer acceptance of later work.

## Epic 3 - Independent native cell treatment

### S2-03 - Present pin state without altering DSP's recipe-grid state

**Status:** Complete - owner accepted on 2026-08-14

**User story:** As a player, I want available and pinned recipes distinguished
subtly while native feedback remains authoritative.

**Definition of done:**

- One non-raycasting tracker-owned clone of the native Replicator background
  grid owns its material instance and state buffer.
- Native filter treatment represents available/unpinned green; native banned-
  color treatment represents pinned red.
- Every refresh derives treatment only from the current populated interactive
  grid and resets every other tracker-buffer entry to neutral, including stale
  entries after repopulation and pins absent from the current grid.
- DSP's original recipe-grid array, material, and compute buffer are never
  written by the tracker.
- The overlay remains beneath recipe icons and is not an interaction surface.
- Refresh occurs only when pin state or native population changes, without
  avoidable per-frame allocation or broad repeated reflection.
- The implementor records a restrained opacity; visual suitability remains for
  later owner validation.
- Missing resources disable only the treatment. Shutdown releases the clone,
  material, buffer, and subscriptions exactly once.
- Tests cover independent ownership, state mapping, repopulation, stale-state
  clearing, changed-only refresh, failure isolation, and cleanup.
- Debug diagnostics record initialization/release and changed-refresh counts
  for populated, unpinned, and pinned cells. They never dump per-cell state;
  tests assert fields and unchanged-state suppression.
- All consumed external members are authority-backed and shim-covered.

**Acceptance gate:** Focused tests, Local/Hosted Release builds, shim
validation, and source review pass, including confirmation of no write path to
the original DSP state buffer. Live appearance and cleanup remain unvalidated.

**Implementation result:**

- `RecipeGridTreatmentModel` owns an independent 120-cell `uint` state array.
  It maps populated unpinned cells to native filter mask `0x2`, populated
  pinned cells to native banned mask `0x8`, and every other cell to neutral.
- Population identities and ordered pins are snapshotted without per-refresh
  allocation. Unchanged requests suppress GPU uploads and diagnostics;
  repopulation clears stale cells even when an absent recipe remains pinned.
- `UnityRecipeGridTreatmentAdapter` clones the native `recipeBg` image once,
  disables raycasting, removes the copied `EventTrigger`, and places the clone
  immediately below `recipeIcons`.
- The clone owns a new material and 120-entry compute buffer. It never reads or
  writes DSP's `recipeStateArray`, `recipeStateBuffer`, or `recipeBgMat`.
- The accepted feasibility calibration is recorded as image opacity `0.08`,
  filter green `(0.20, 0.75, 0.25, 1.00)`, and banned red
  `(0.78, 0.22, 0.22, 0.45)`. Live suitability remains an owner observation at
  the later combined gate.
- Missing population or Unity resources disable only this treatment. Partial
  and normal cleanup release the owned buffer, material, and clone once. S2-03
  introduces no event subscription.
- Bounded Debug diagnostics record initialize/release and changed refresh
  counts only; no per-cell data is logged.

**Technical validation:** Passed on 2026-08-14 with:

```powershell
.\scripts\validate\Validate-S2-03.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>'

$revision = git rev-parse HEAD
.\scripts\validate\Validate-S1-06.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 9 `
  -SourceRevision $revision
```

Focused tests covered independent state ownership, green/red/neutral mapping,
pin changes, population replacement, stale clearing, absent pins, unchanged
suppression, missing-resource isolation, inert failure, one-time cleanup, and
bounded changed-count diagnostics. Read-only metadata validation reconfirmed
the hash-matched 120-cell native layout, public image fields, private native
state resources, and native `_StateBuffer` binding. Static inspection found no
production reference or reflection string for the original native state array,
buffer, or material. Local and Hosted Release builds completed with zero
warnings and zero errors; both products passed exhaustive consumed-surface
coverage against exact declaration-only shims. No game, loader, save, plugin,
or substitute runtime was started or changed.

**Owner acceptance:** Accepted explicitly on 2026-08-14. This acceptance
authorized S2-04; it does not infer acceptance of later work.

## Epic 4 - HUD visibility and controls

### S2-04 - Supply the exact major-interface visibility input

**Status:** Complete - owner accepted on 2026-08-15

**User story:** As a player, I want the tracker hidden during the six agreed
major interfaces without unrelated windows changing my choice.

**Definition of done:**

- A runtime adapter reads only Tech, Dyson Editor, Inventory, Replicator,
  Statistics, and Dashboard and returns their logical OR.
- It returns availability separately from the active value and never converts
  unavailable bindings into an invented `false`; consuming orchestration hides
  the tracker while unavailable without overwriting manual intent.
- It does not redefine the existing visibility policy or expand the six-window
  rule by inference.
- Bindings are narrow, cached, and fail softly without six lifecycle patches or
  repeated broad reflection.
- Tests cover every individual signal, simultaneous signals, all false,
  unavailable bindings, fail-closed presentation, and the collection/policy
  boundary.
- Debug diagnostics record binding availability once and only changes to the
  combined state, naming active members from the fixed set. Tests assert level,
  fields, availability transitions, and unchanged-state suppression.
- All consumed members are authority-backed and fully shim-covered.

**Acceptance gate:** Focused tests, Local/Hosted Release builds, exhaustive shim
validation, and source review pass. Live window state remains unvalidated.

**Implementation result:**

- `MajorInterfaceSignals` represents exactly Tech, Dyson Editor, Inventory,
  Replicator, Statistics, and Dashboard and derives their logical OR without
  admitting unrelated DSP window state.
- `UnityMajorInterfaceStateAdapter` caches the six public `UIGame` window
  references once and reads their inherited public `ManualBehaviour.active`
  getters directly. It uses no reflection, Harmony patch, broad window proxy,
  or per-window lifecycle subscription.
- `MajorInterfaceSnapshot` preserves binding availability separately from the
  active value. The policy handoff hides presentation while unavailable and
  otherwise delegates unchanged manual intent and row state to
  `VisibilityPolicy`.
- Missing references or read failures return unavailable and fail softly.
  They do not invent an inactive runtime state or mutate manual visibility.
- Bounded Debug diagnostics report the first availability observation,
  availability transitions, and combined active/inactive transitions only.
  Active diagnostics name members from the fixed six-name set; changing the
  active member while the logical OR remains true is deliberately silent.

**Technical validation:** Passed on 2026-08-14 with:

```powershell
.\scripts\validate\Validate-S2-04.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>'

$revision = git rev-parse HEAD
.\scripts\validate\Validate-S1-06.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 10 `
  -SourceRevision $revision
```

Focused tests covered each individual signal, simultaneous signals, all false,
unavailable and throwing adapters, fail-closed presentation, unchanged policy
ownership, availability transitions, combined-state transitions, fixed member
names, Debug level, bounded messages, and unchanged suppression. Read-only
metadata validation confirmed the hash-matched public `UIGame` fields, each
window's `ManualBehaviour` inheritance, and the public Boolean `active` getter.
Local and Hosted Release builds completed with zero warnings and zero errors;
both products passed exhaustive consumed-surface coverage against exact
declaration-only shims. No game, loader, save, plugin, or substitute runtime
was started or changed.

**Owner acceptance:** Accepted explicitly on 2026-08-15. This acceptance
authorized S2-05; it does not infer acceptance of later work. Live window
state remains unvalidated until the later meaningful owner gate.

### S2-05 - Connect native recipe-icon slots to the panel

**Status:** Active - implemented and technically validated; owner acceptance
pending

**User story:** As a player, I want my ordered pins represented by recognizable
native recipe icons in the tracker shell.

**Definition of done:**

- A narrow runtime adapter resolves each recipe ID and reads
  `RecipeProto.iconSprite` directly without selecting an output item or
  reinterpreting multiple-output recipes.
- At most three ordered slots show only those icons. Sprint 2 adds no recipe
  name, numeric ID, ingredient, inventory, quantity, sufficiency, or machine-
  only placeholder content.
- A missing recipe or icon requests safe invalid-recipe removal, preserves the
  remaining order, and emits one bounded diagnostic for that identity.
- Slot presentation consumes tracker state without owning its rules and never
  writes DSP state. It cannot pin, unpin, or navigate.
- The drag region uses accepted geometry, remains clamped, and contains input.
- Missing hosts/resources disable only affected presentation; partial
  initialization and one-time owned-resource release are safe.
- Tests cover slot synchronization, multiple-output recipes, missing recipe and
  icon removal, preserved order, maximum-three presentation, drag/clamp pass-
  through, failure isolation, cleanup, and inert post-release behavior.
- Debug diagnostics record initialization/release and changed slot order as at
  most three recipe IDs. Failure messages are once per identity; tests assert
  level, fields, bounds, and unchanged-state suppression.
- All production external references are authority-backed and shim-covered.

**Acceptance gate:** Focused tests, Local/Hosted Release builds, shim validation,
and source review pass. Review confirms direct `RecipeProto.iconSprite` use and
separation of resolution, tracker state, and Unity ownership. Live composition,
dragging, containment, and cleanup remain unvalidated.

**Implementation result:**

- `UnityRecipeIconResolver` performs the exact direct lookup
  `LDB.recipes.Select(recipeId)` and reads only `RecipeProto.iconSprite`. It
  does not inspect recipe outputs, choose an output item, navigate, or attach
  input behavior.
- `RecipeIconSlotPresentation` resolves the current ordered pins into at most
  three opaque icon handles. The UI-independent synchronizer owns no Unity
  object and delegates invalid-identity removal to `PinnedRecipeState`.
- A missing recipe, missing sprite, or failed lookup removes only that identity
  through `RemoveUnavailable`, preserves valid relative order, and reports the
  identity once. Unchanged slot order suppresses UI work and refresh logging.
- `TrackerPanelUiBoundary` accepts the resolved slot frame without taking over
  recipe resolution. Slot application or cleanup failure is isolated from the
  panel shell, its visibility, and its existing clamped drag boundary.
- `UnityTrackerPanelAdapter` lazily creates three tracker-owned 52-by-52
  non-raycasting `Image` slots in fixed ordered rows. The panel background
  remains the raycasting containment surface, and the panel boundary retains
  its clamped drag-application path. Slot UI cannot pin, unpin, navigate, or
  pass clicks through independently.
- Partial creation and normal shutdown destroy only slot-owned objects once.
  A failed slot layer becomes inert while the panel shell remains available.
- Bounded Debug diagnostics record initialize/release, changed recipe-ID order,
  one failure per invalid identity, and isolated slot disablement. They contain
  no names, recipe payloads, icon data, or other large dumps.

**Technical validation:** Passed on 2026-08-15 with:

```powershell
.\scripts\validate\Validate-S2-05.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>'

$revision = git rev-parse HEAD
.\scripts\validate\Validate-S1-06.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 11 `
  -SourceRevision $revision
```

Focused tests covered ordered three-slot synchronization, unchanged
suppression, unpinning, direct identity treatment for a multiple-output recipe,
missing recipe and sprite removal, preserved valid order, repeated invalid
identity suppression, resolver exceptions, unavailable panel hosts, isolated
slot-UI failure, inert post-release behavior, one-time cleanup, clamped drag
pass-through, input containment, and bounded Debug diagnostics. Read-only
metadata validation confirmed the hash-matched static `LDB.recipes` getter,
`RecipeProtoSet -> ProtoSet<RecipeProto>` inheritance,
`ProtoSet<T>.Select(int)`, and the public `RecipeProto.iconSprite` getter.
Local and Hosted Release builds completed with zero warnings and zero errors;
both products passed exhaustive consumed-surface coverage against exact
declaration-only shims. Static review rejected output arrays, item lookup,
navigation, click listeners, Harmony, and Unity dependencies in the slot
synchronizer. No game, loader, save, plugin, or substitute runtime was started
or changed.

**Owner acceptance:** Granted on 2026-08-15. Live icon composition, dragging,
containment, and cleanup remain unvalidated until the later meaningful owner
gate.

### S2-06 - Add paired visibility controls and complete orchestration

**Status:** Implemented and technically validated - owner acceptance pending

**User story:** As a player, I want explicit controls for hiding and restoring
the integrated tracker without automatic conditions changing my choice.

**Definition of done:**

- Startup resolves native hosts/resources once and connects tracker state,
  Replicator gesture and treatment, recipe-icon slots, the major-interface
  input, `VisibilityPolicy`, and `TrackerPanelUiBoundary` without moving rules
  into Unity code.
- `manualRequested` starts true each session. Panel Hide sets it false; a
  Show/Hide control under `UIGame.gameMenu` toggles stored intent rather than
  computed visibility. Empty and automatic/unavailable states never overwrite
  that intent.
- The implementor records a native global-control icon and concise English
  fallback copy. Baseline copy is `Recipe Tracker`, `Hide Recipe Tracker`, and
  `Show Recipe Tracker`; no broader localization table is introduced.
- Cloned controls discard inherited listeners, tooltip content, and unrelated
  localization. An accurate tooltip may be omitted.
- Empty state and the exact visibility policy are applied; unavailable major-
  interface bindings hide the panel. Other missing resources fail per feature.
- Shutdown removes all tracker listeners and releases only tracker-owned
  resources once; partial initialization is safe and callbacks become inert.
- Tests cover initial/manual intent, stored-versus-computed toggling, empty-
  state retention, six-interface and unavailable-state hiding, visibility
  pass-through, partial initialization, cleanup, and inert release.
- Debug diagnostics record initialization/release, manual actions, and changed
  visibility with `hasRows`, `manualRequested`, major-interface availability,
  and active state. Tests assert level, fields, and unchanged-state suppression.
- All production external references are authority-backed and shim-covered.

**Acceptance gate:** Focused tests, Local/Hosted Release builds, exhaustive shim
validation, package construction, and static inspection pass. Architecture
separation is reviewed. A testable development DLL and deferred human-only
claim list are prepared; no runtime procedure is run in Sprint 2.

**Implementation:** Completed on 2026-08-15.

- `DSPRecipeTrackerPlugin` waits for the native UI root, then resolves the
  in-game UI host, Replicator, major-interface, game-menu template, and icon resources
  once. It composes the existing plain state and presentation boundaries; its
  frame callback only asks the orchestrator to refresh cached integrations.
- `TrackerOrchestrator` owns session-only `manualRequested`, starts it true,
  applies the existing visibility policy, and keeps automatic or unavailable
  UI conditions from rewriting player intent. Feature initialization and
  cleanup remain independent and one-time.
- The panel Hide control and global control are tracker-owned clones of
  `UIGame.gameMenu.buttonS`. Their inherited click event is replaced, inherited
  localization components are removed, and tooltip content is reset before
  tracker behavior is attached.
- Both controls use the non-raycasting icon sprite within the live native
  Replicator `UIGame.gameMenu.button3` control. Tracker-owned fallback copy is `Recipe Tracker`,
  `Hide Recipe Tracker`, and `Show Recipe Tracker`; no localization table or
  persistence was added.
- Debug diagnostics report orchestration initialization/release, manual
  actions, and changed computed visibility with the required bounded fields.

**Technical validation:** Passed on 2026-08-15 with:

```powershell
.\scripts\validate\Validate-S2-06.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>'

$revision = git rev-parse HEAD
.\scripts\validate\Validate-S1-06.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 13 `
  -SourceRevision $revision
```

Focused tests covered initial intent, panel Hide, global toggling while
automatically hidden, empty-state retention, exact major-interface hiding and
recovery, unavailable-state hiding, visibility pass-through, unchanged-state
suppression, partial initialization, inert released callbacks, one-time
cleanup, and bounded Debug diagnostics. Local and Hosted Release builds
completed with zero warnings and zero errors; exhaustive consumed-surface
coverage passed against declaration-only shims. Read-only authority validation
confirmed `UIRoot.instance`, the in-game and game-menu hosts, `buttonS`, the
native Replicator `button3` sprite path, click-event replacement, tooltip
fields, localization removal, and the consumed Unity control members. Static
inspection confirmed that Unity owns only lifecycle, resource binding, and UI
callbacks while stored intent and visibility composition remain plain C#.

The real version-aligned development DLL and package were constructed under
ignored `artifacts/` and passed static package inspection. No game, loader,
save, plugin, or substitute runtime was started, installed, or changed.

**Deferred human-only claims:** Native gesture coexistence, recipe-grid opacity,
icon and fallback-copy suitability, live panel composition, dragging and input
containment, exact six-interface behavior, paired Show/Hide interaction, and
live cleanup remain for the coherent Sprint 3 owner gate. The development DLL
is testable but is not installed, behaviorally ready, or publication-ready.

**Owner acceptance:** Granted on 2026-08-15. Story acceptance does not infer
Sprint exit owner review.

## Sequence and dependencies

```text
S2-01 transient tracker state
    |
    +--> S2-02 native right-click integration --> S2-03 cell treatment
    +--> S2-04 exact major-interface adapter
    `--> S2-05 native recipe-icon panel slots

S2-01 through S2-05
    |
    `--> S2-06 paired controls and final orchestration
```

Stories are activated one at a time. The structure proves state rules without
Unity, then isolates each fragile runtime boundary before final orchestration.

## Sprint exit gates

### Source-ready

**Status:** Passed on 2026-08-15 for commit
`b5ca0c3d9b51e586f08cc0347d95649bc4edcb62`.

- Every accepted Sprint 2 story meets its definition of done.
- Focused tests and Local/Hosted Release builds pass with zero errors.
- Every production external reference is completely represented by the
  machine-readable inventory and declaration-only shims with no extra surface.
- Static review confirms read-only game state, native listener preservation,
  independent treatment, bounded failure, useful bounded diagnostics, and
  complete owned-resource cleanup paths.
- Documentation separates deterministic evidence from unknown live behavior.

**Evidence:**

- `Validate-S2-06.ps1` passed the complete S2-01 through S2-06 focused
  acceptance chain against the recorded authority surfaces.
- `Validate-S1-06.ps1` with build number `14` passed deterministic tests and
  Local/Hosted `Release` builds with zero warnings and zero errors. Both
  products exposed the same 118 normalized external type/member references
  and passed exhaustive coverage against the declaration-only shims.
- Static inspection found no game/save mutation API, persistence path,
  Harmony dependency, generated output, dependency binary, save, log, or
  secret in tracked source. Owned listeners and resources have bounded cleanup
  paths and changed-state diagnostics remain bounded.
- GitHub Actions run
  [`31847803100`](https://github.com/shytamir/DSPRecipeTracker/actions/runs/31847803100)
  checked out the exact accepted commit and completed the hosted inspected-
  package workflow successfully.

No Source-ready evidence installed, loaded, or started DSP, BepInEx, Unity,
Steam, a save, or a substitute runtime. This gate establishes no runtime,
visual, interaction, cleanup, or compatibility behavior.

### Package-inspected

**Status:** Passed on 2026-08-15 for package `0.1.14` built from commit
`b5ca0c3d9b51e586f08cc0347d95649bc4edcb62`.

- The real version-aligned Release output passes existing local and hosted
  static package inspection.
- No shim, dependency binary, generated evidence, save, log, screenshot, or
  secret enters the package.
- Metadata remains explicit that the development artifact is not installed,
  in-game validated, or release-ready.

**Evidence:**

- `Build-S1-03.ps1` passed Local and Hosted construction and static inspection
  with build number `14` and the exact accepted source revision.
- The archive contains exactly `manifest.json`, `README.md`, `icon.png`, and
  `BepInEx/plugins/DSPRecipeTracker/DSPRecipeTracker.dll`.
- Package, semantic, assembly, file, diagnostic, GUID, display-name, and DLL-
  hash checks passed. The archive SHA-256 is
  `8a1eb1fd61ced6ec5efb37b18984d16b49c4feb11f8683222a759cca0434adb0`.
- No shim, dependency assembly, PDB, build input, save, log, screenshot,
  secret, or player-specific path entered the archive.
- The packaged README states that the development artifact is not installed,
  in-game validated, or release-ready.

Package inspection performed no installation or runtime load and establishes
no Behavioral-ready, compatibility, publication-ready, or supported-release
claim.

### Owner-reviewed

**Status:** Passed on 2026-08-15.

- The owner reviews story and sprint evidence, deferred live claims, and known
  limitations.
- Owner review does not imply Behavioral-ready, compatibility, or publication
  approval.

The owner explicitly accepted S2-06 and the Sprint 2 exit after reviewing the
Source-ready and Package-inspected evidence. This closes Sprint 2 without
making an installed, runtime, visual, interaction, compatibility, publication,
or supported-release claim.

## Explicitly unavailable states

Sprint 2 cannot establish `Behavioral-ready`, installed, visual, interaction,
runtime cleanup, compatibility, or `Publication-ready` status. Those claims
cannot be inferred from builds, doubles, shim coverage, package inspection,
historical feasibility, or source review.

Human-only checks for gesture behavior, native-state coexistence, opacity,
icon/text suitability, layout, dragging, containment, six-interface hiding,
Show/Hide behavior, and live cleanup remain deferred to the coherent owner gate
in Sprint 3.

## Decisions and change control

The implementor selects and records cell-treatment opacity, the native
Show/Hide icon, and concise tracker-owned English fallback copy within S2-06.
These choices receive source/static validation first and later human validation
through a testable DLL.

Tracker-icon navigation remains excluded and decision-gated for Sprint 3.
Supported resolution/UI-scale scope must be resolved before its consuming
Sprint 3 story becomes Ready.

An owner-approved roadmap change is required to alter pin gesture, eligibility,
ordering, capacity, lifetime, the six-interface rule, player-owned visibility,
independent cell treatment, runtime-validation boundary, or a deferred feature.
