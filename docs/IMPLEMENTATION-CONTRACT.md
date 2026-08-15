# DSP Recipe Tracker - Implementation Contract

## Status and authority

**Status:** Current implementation contract

**Implementation status:** Sprints 1 through 3 and their required owner gates
are complete and owner accepted. Sprint 3 produced a working MVP prototype.
The owner passed the bounded Behavioral-ready procedure and accepted direct
workshop build `0.1.318`; Source-ready and Package-inspected then passed for
clean source revision `14bbe8e046e32333bd7cf68f35b8f22bc04dd47f` using build
number 318. The repository is planning-pending for publication refinement; no
supported or published release exists.
The source tree contains the minimal BepInEx identity/lifecycle/logging
skeleton, static package pipeline, UI-independent panel geometry and visibility
policy, an inert compile-time Unity panel boundary, deterministic transient pin
state, an isolated native Replicator input adapter, independent native recipe-grid
treatment model and adapter, exact fail-closed major-interface visibility
input, ordered complete native-composed row presentation, paired tracker-owned
visibility controls, and plain orchestration connected through the plugin
lifecycle, plus a UI-independent direct-ingredient, Icarus-count, sufficiency,
and machine-warning presentation model, separate direct DSP recipe/item and
Icarus-inventory adapters, a normalized input source, bounded live refresh,
and feature-isolated live scale-aware panel dragging and parent-bound
reclamping connected through plugin orchestration. Game-session shutdown
releases the orchestrator so transient pins and owned UI cannot cross a same-
or different-save load.

**Owner review:** Accepted through 2026-08-15.

This file governs implementation structure, runtime integration mechanics,
lifecycle ownership, failure handling, and the source toolchain. It implements
the product behavior defined by [`PROJECT.md`](PROJECT.md) without changing
that behavior.

Use these sources for their respective authority:

- [`PROJECT.md`](PROJECT.md) for product behavior and scope;
- [`PRODUCT-PRINCIPLES.md`](PRODUCT-PRINCIPLES.md) for the native-extension
  design;
- [`FEASIBILITY.md`](FEASIBILITY.md) for confirmed game members and historical
  runtime findings;
- [`RUNTIME-AUTHORITY.md`](RUNTIME-AUTHORITY.md) for read-only runtime
  inspection and consumed-surface boundaries;
- [`BEPINEX-CONFORMANCE.md`](BEPINEX-CONFORMANCE.md) for loader conformance;
- [`THUNDERSTORE-PACKAGE.md`](THUNDERSTORE-PACKAGE.md) for package layout,
  versioning, hosted compile references, and package validation;
- [`VALIDATION-CONTRACT.md`](VALIDATION-CONTRACT.md) for readiness and evidence
  claims; and
- [`ROADMAP.md`](ROADMAP.md) for current planning state and operational
  authorization, when any.

This contract does not activate a story, authorize runtime execution, record
validation, or approve a release.

## 1. Architecture

Keep these responsibilities separate:

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

### 1.1 Runtime adapters

- Read version-sensitive game state and isolate direct DSP access.
- Resolve recipes, direct inputs, inventory quantities, UI hosts, and native
  resources.
- Cache fragile bindings outside update loops.
- Return explicit bounded absence or failure rather than inventing game state.
- Do not own pin order, eviction, sufficiency rules, or Unity layout.

### 1.2 Tracker state

- Own recipe identities, toggle behavior, order, the three-entry capacity, and
  FILO eviction.
- Use deterministic state transitions that can be tested without DSP or Unity.
- Do not manipulate the Unity hierarchy.
- Do not read or write save, inventory, crafting, or factory state.

### 1.3 Presentation model

- Convert resolved recipe and inventory state into UI-ready row values.
- Own direct-ingredient sufficiency calculations and machine-warning values.
- Do not discover runtime members or manipulate the Unity hierarchy.

### 1.4 Replicator integration and Unity UI

- Own native event subscription, tracker-owned Unity objects, layout,
  dragging, clamping, input capture, visibility application, and cleanup.
- Consume tracker state and presentation values without redefining product
  rules.
- Do not collect runtime state that belongs in an adapter.
- Avoid repeated broad reflection scans and avoid avoidable per-frame
  allocation.

## 2. Replicator integration

### 2.1 Pin gesture

- Use the existing Replicator recipe-grid `PointerDown` path confirmed in the
  feasibility record.
- Preserve DSP's native selection callback and add tracker behavior after the
  native path.
- Filter tracker behavior to right-click and resolve the recipe from the same
  current grid index and recipe array used by DSP.
- Left-click must not enter tracker state.
- Resolve and cache version-sensitive array and index bindings outside update
  loops.
- Remove the tracker listener during shutdown.
- The confirmed MVP gesture requires no Harmony patch. HarmonyX must not be
  referenced merely because it is available.

### 2.2 Eligibility

- Treat a native populated and interactive recipe cell as the primary
  eligibility boundary.
- A diagnostic unlock query may reject inconsistent state but must not expose
  or admit a recipe absent from the native grid.
- Do not create a parallel recipe list or technology-lock calculation.

### 2.3 Independent cell treatment

- Keep available, unpinned recipe cells neutral.
- Represent at most three pinned cells with tracker-owned marker objects, each
  composed from four green corner brackets over the native 14-by-8 grid.
- Keep every marker segment non-raycasting and restrained so recipe artwork and
  native hover, selection, disabled, and machine-only states remain legible.
- Never write tracker state into DSP's original recipe-grid buffer.
- Reposition or hide markers when pin state or native grid population changes.
- Release all marker objects and the input listener during shutdown.

The earlier cloned-material/full-cell design remains feasibility history, not
the implementation contract. Runtime testing showed that its visual cost and
complexity outweighed the value of treating every available cell. The accepted
pinned-corner design preserves immediate pin recognition without obscuring
icons or asking the player to decode another state.

## 3. Recipe and inventory integration

- Store transient recipe identities as integer runtime recipe IDs.
- Resolve a recipe through the current runtime data source when presentation
  refreshes.
- Read direct ingredient IDs and counts only.
- Read Icarus inventory counts without moving or reserving items.
- The supported baseline contains one through six direct ingredients per
  recipe; deterministic modeling and fixed-row layout cover that full range.
- Use the game's hand-craftability and localized production-category values
  for machine-only presentation.
- Reuse live recipe and item icons rather than copying game art.
- Treat modded-recipe support as an evidence-driven compatibility question,
  not a promise. A conforming registered recipe may work through the same
  adapters without a special compatibility layer.

## 4. Tracker panel integration

- Create tracker-owned UI instances composed from live native resources.
- Do not assume that a native Replicator recipe cell is a cloneable Unity
  `Button`; the confirmed implementation uses a shared rendered grid.
- Resolve native hosts and resources once and cache them.
- Do not use asset names as the sole loading contract.
- If a required host or resource is absent, skip the affected presentation and
  emit one bounded diagnostic rather than creating a bespoke fallback.
- Keep geometry and clamping calculations UI-independent where practical.
- Apply the single visibility result produced by the visibility policy; Unity
  code must not maintain a second rule.
- Add the global control beneath the existing bottom-right
  `UIGame.gameMenu` group so it inherits that group's visibility and lifecycle.
- Clear inherited listeners, tooltip data, and unrelated localization behavior
  from any cloned native control before assigning tracker behavior.
- Tracker-owned raycast targets must contain input without preventing the
  designated drag interaction.
- The panel shell uses a flat dark translucent raycast-containment surface with
  no border, sprite, texture, asset-name lookup, or Replicator-grid resource
  dependency.
- The fixed panel is 360 by 300 UI-layout units. Three 90-unit rows follow a
  semantic heading band; each row reserves a 30-unit full-width footer for its
  normalized machine/facility name.
- The increased height is an owner-accepted compromise after native multiline
  text repeatedly clipped or disappeared. It may be reviewed during future
  publication refinement, but interactive resizing remains outside the MVP.
- Initial placement derives true left-middle top position from the live parent
  height. The global control uses the native screenshot button as its reference
  and is offset by `(0, 38)` to avoid the large Tech control.

## 5. Data and state safety

- Treat DSP, Icarus inventory, crafting, factory, and save state as read-only.
- Do not craft, replicate, move items, reserve materials, place buildings, or
  write tracker data to a save.
- Do not use BepInEx configuration as inferred per-save storage.
- Session recipe IDs, visibility choice, and panel position remain transient
  in the MVP.
- Product rules belong in tracker state or presentation code, not reflection
  bindings or Unity callbacks.

## 6. Failure and cleanup contract

- A missing recipe, inconsistent direct-input arrays, or missing required
  item/icon removes that invalid pin while preserving the relative order of
  valid pins, consistent with the accepted Sprint 2 failure rule.
- An unavailable player/package, production-category value, presentation
  resource, UI host, or member fails softly. Retain the pin and suppress the
  complete affected row until valid data returns rather than showing partial or
  invented requirements.
- Bound repeated diagnostics by failure identity; do not emit unbounded log
  noise during refresh.
- An unavailable integration hides or skips only the affected presentation.
- Do not block the Replicator, corrupt tracker state, or alter native game
  interaction after an integration failure.
- Release tracker-owned event subscriptions, Unity objects, materials,
  compute buffers, and other resources during plugin shutdown.
- Subscribe once to the game-ended lifecycle event, release the complete
  orchestrator there, and permit clean initialization for the next game
  session. Session state must not survive same-save or different-save loads.

## 7. Source and dependency baseline

The initial source baseline is:

- C# compatible with BepInEx 5;
- .NET Framework 4.7.2 (`net472`);
- BepInEx `5.4.17.0` and its documented minimal feature surface;
- Dyson Sphere Program `0.10.34.28529`, Steam build `23109513`; and
- Unity `2022.3.62f3c1`.

These versions identify the recovered source baseline. They do not promise
compatibility with later game, Unity, BepInEx, Harmony, or third-party mod
versions.

Game, Unity, and BepInEx assemblies remain external dependencies. Local build
configuration receives game and Unity inputs through the explicit
maintainer-supplied `GameRoot` setting defined by `THUNDERSTORE-PACKAGE.md`.
BepInEx may come from that root or from an explicit documented
`BepInExReferencePath`; neither path is discovered silently. Hosted builds use
the contract's minimal
source-defined game and Unity compile-reference shims and the documented
BepInEx CI source. Source and package construction must not copy, commit, or
redistribute installed assemblies. Compile-reference shims contain declaration
shapes only and are not runtime substitutes.

`Directory.Build.props` exposes the authorized local reference paths only when
the caller explicitly selects `DSPReferenceMode=Local` and supplies
`GameRoot`; an explicitly supplied BepInEx path overrides only that compile
input. Hosted builds explicitly select `DSPReferenceMode=Hosted` and use
the fixed repository-owned `ci/compile-references` root. Consuming projects set
`DSPRecipeTrackerRequiresExternalReferences=true`; the shared target then fails
with an ordinary MSBuild error when the mode, root, surface inventory, or any
authorized local assembly is missing. The shared configuration contains no
discovery, download, copy, installation, or process-execution behavior. The
S1-02 build script generates all version forms from `VERSION`, an explicit
build number, and an explicit source revision, then builds without copying,
installing, or loading the product assembly.

Use only the BepInEx lifecycle, identity, logging, and explicitly required
configuration surface documented in `BEPINEX-CONFORMANCE.md`. Do not target
BepInEx 6, add speculative loader adapters, or consume loader internals.

## 8. Adopted repository layout

The completed prototype uses this structure:

```text
DSPRecipeTracker.sln
src/
  DSPRecipeTracker/
    DSPRecipeTracker.csproj
tests/
  DSPRecipeTracker.Tests/
    DSPRecipeTracker.Tests.csproj
ci/
  compile-references/
    DSPGame.Reference/
    Unity.Reference/
    surface-inventory.json
scripts/
  build/
  validate/
packaging/
  manifest.json
  README.md
  icon.png
```

- `src/DSPRecipeTracker` produces the single shipping
  `DSPRecipeTracker.dll`. Runtime adapters, tracker state, presentation model,
  and Unity UI remain separate in code without creating additional shipping
  assemblies for the MVP.
- `tests/DSPRecipeTracker.Tests` contains deterministic source-level tests and
  does not load DSP, BepInEx, Unity, or a substitute runtime.
- `ci/compile-references` contains declaration-only hosted-build shims and the
  machine-readable consumed-surface inventory. `DSPGame.Reference` emits the
  required game assembly identity; `Unity.Reference` may contain one internal
  project per consumed Unity assembly identity.
- `scripts/build` owns deterministic build and package orchestration.
  `scripts/validate` owns shim-coverage, assembly, version, and archive checks.
- `packaging` contains tracked Thunderstore inputs only. Generated binaries,
  archives, reports, and intermediate output remain under ignored paths.

Future work must not add empty placeholder projects or directories before its
roadmap story is Active. The structure remains a placement contract, not
authorization to implement later work.

## 9. Adopted presentation decisions

The completed prototype adopted:

- a fixed 360-by-300 panel with a full-width facility footer;
- pinned-only green corner markers with neutral unpinned cells;
- the native Replicator icon for paired visibility controls;
- a borderless dark translucent panel background;
- native font reuse with 12-unit headings, 13-unit quantities, and 11-unit
  facility text; and
- true left-middle initialization plus a `(0, 38)` global-control offset from
  the native screenshot button.

Deterministic checks covered geometry, state, cleanup, and exact consumed
surfaces. The owner judged live clarity, native fit, lifecycle, and usability
through the bounded procedure and refinement workshop. The archived
[`Prototype Roadmap`](archive/PROTOTYPE-ROADMAP.md) preserves the progression
and rationale without turning intermediate experiments into current contracts.

## 10. Implementation change control

An implementation change requires product review when it would change a rule
in `PROJECT.md`. It requires new feasibility or authority evidence when it
would:

- consume a new game or Unity member not preserved by current evidence;
- add another Unity module or loader dependency;
- introduce Harmony or another patching/detour mechanism;
- broaden runtime collection or compatibility behavior; or
- substitute a different native integration surface.

Implementation convenience does not override product behavior, the active
roadmap scope, or the runtime authorization boundary.
