# Sprint 3 Roadmap - Recipe Presentation and Prototype Hardening

## Status and authority

**Status:** Authorized - implementation underway

**Active story:** S3-06 - Complete orchestration and prepare the owner checkpoint

**Active story state:** Implementation complete; exact-revision technical
validation pending

**Implementation authorization:** S3-06 only

**Parent roadmap:**
[`DSP Recipe Tracker - MVP Roadmap`](planning/MVP-ROADMAP.md)

**Previous roadmap:**
[`Sprint 2 - Native Integration and Tracker State`](archive/UI_INTEGRATION_ROADMAP.md)

The owner authorized this Sprint 3 roadmap on 2026-08-15. S3-01 through S3-05
are owner accepted. Implementation is underway with S3-06 as the only Active
story. Sprint completion remains unavailable pending its technical handoff,
the owner-performed Behavioral-ready gate, and explicit owner review.

**Goal:** Complete live direct-ingredient and inventory presentation, harden
the integrated tracker, and prepare the working MVP prototype for one bounded
owner-performed human validation gate.

**Exit result:** The source prototype completes the MVP loop:

```text
right-click recipe -> pin state -> native-composed recipe row
                   -> direct ingredients and Icarus inventory comparison
                   -> visibility controls -> unpin or FILO eviction
```

The agent-delivered result is a deterministic, statically inspected, testable
DLL and package plus a concise owner procedure. Installed behavior is not
claimed until the owner performs and reports the human-only checks.

## Safety and validation boundary

- Agents and automation do not install or execute the plugin, launch DSP or a
  substitute runtime, open a save, automate input, or collect owner-environment
  logs or screenshots.
- DSP recipe, inventory, crafting, factory, and save state remain read-only.
- Deterministic rules, arithmetic, layout calculations, state transitions,
  package contents, and external-reference coverage are validated without a
  runtime.
- Human validation is reserved for the final coherent checkpoint and only for
  native interaction, visual composition, live layout, containment, lifecycle,
  and cleanup claims that cannot be established another way.
- Every story adds concise Debug diagnostics useful for interpreting its later
  human observations. Diagnostics report bounded identities, counts, states,
  or failure reasons rather than runtime objects or data dumps.
- Story technical validation and explicit owner acceptance remain separate.
  Neither activates the next story by inference.

## Entry gates and resolved decisions

The following accepted baseline is available:

- Sprint 1 and Sprint 2 are complete and owner accepted.
- The current source and inspected package pass their recorded deterministic,
  Local/Hosted Release, shim-coverage, and static package gates.
- Sprint 2 supplies transient ordered pins, native right-click input,
  independent cell treatment, exact six-interface visibility, three ordered
  native product-icon slots, paired visibility controls, and startup
  orchestration.
- Installed gesture behavior, native-state coexistence, opacity, icon/copy
  suitability, panel composition, dragging, containment, live visibility, and
  cleanup remain unvalidated and are intentionally grouped into the Sprint 3
  owner checkpoint.
- The accepted Phase 1 findings contain 161 exact runtime recipes and 441
  direct-input edges. Every recipe has one to six direct ingredients; the
  maximum is six for recipe 75, Universe Matrix.
- Read-only inspection of the matching game and Unity assemblies confirms the
  exact recipe, inventory, Auto layout-height, canvas-scale, parent-rectangle,
  and pointer-drag members consumed by the proposed stories.

The owner resolved the Sprint 3 entry decisions as follows:

1. **Tracker-icon navigation:** excluded from the MVP. Product and ingredient
   icons remain non-interactive tracker presentation.
2. **Display validation:** deterministic validation covers 1920-by-1080/Auto at
   UI layout height 1080 and 2560-by-1440/Auto at UI layout height 1440. Owner-
   performed runtime validation covers only 3840-by-2160/Auto at UI layout
   height 1080. No other resolution or UI-scale support claim is made by the
   MVP.

Implementors may choose bounded internal layout, refresh, formatting, and
diagnostic details that preserve the contracts. Those choices receive
deterministic checks first and human judgment only at the final gate.

## Sprint boundaries

### Included

- UI-independent direct-ingredient, inventory, sufficiency, and machine-only
  presentation modeling.
- Narrow read-only DSP recipe, item-icon, and Icarus-inventory adapters.
- Native-composed product and ingredient rows for at most three pins.
- Live inventory refresh without pin reordering or avoidable frame-by-frame
  allocation.
- Fixed-panel layout, dragging, screen clamping, input containment, failure
  isolation, and owned-resource cleanup hardening.
- A real versioned test DLL and package, complete non-runtime evidence, and one
  concise owner-performed human procedure.

### Excluded

- Quantity scaling, saved pins or panel state, automatic replication,
  recursive ingredient expansion, broader inventory scopes, animation, panel
  resizing, and speculative compatibility layers.
- A parallel recipe browser, independent unlock model, second pinning surface,
  or tracker-driven crafting or inventory mutation.
- Broad localization tables or replacement game art.
- Tracker-icon navigation or any other tracker-row interaction.
- Installed or in-game execution by agents or automation.
- Publication, compatibility, supported-release, or general display-support
  claims not established by their explicit gates.

## Epic 1 - Presentation data

### S3-01 - Model direct requirements and inventory sufficiency

**Status:** Complete - owner accepted on 2026-08-15

**User story:** As a player, I want each pin converted into a stable row model
that tells me which direct ingredients are required and whether Icarus carries
enough for one recipe operation.

**Definition of done:**

- A UI-independent presentation component consumes normalized recipe and
  inventory inputs and produces ordered UI-ready rows without referencing DSP,
  Unity, BepInEx, or reflection.
- Each row preserves the exact pinned recipe ID and carries the opaque native
  recipe-icon handle supplied by the adapter without selecting or
  reinterpreting an output item. It also contains direct ingredient IDs and
  icon handles, required counts, current Icarus counts, and sufficiency values.
- Requirements represent one unscaled recipe operation. `current >= required`
  is sufficient; a lower count is insufficient.
- Ingredient order follows the normalized runtime recipe input order. The
  model does not expand ingredients recursively, combine other inventory
  scopes, reserve items, or simulate crafting.
- The model supports one through six direct ingredients. The Phase 1 baseline
  contains 27 one-input, 43 two-input, 42 three-input, 44 four-input, four
  five-input, and one six-input recipe; it contains no zero-input recipe or
  duplicate recipe/input pair.
- Machine-only rows carry the normalized native production-category text as a
  warning value. Hand-craftable rows carry no machine warning.
- Invalid or structurally inconsistent inputs produce an explicit bounded
  absence/failure result and cannot corrupt the remaining ordered rows.
- Focused deterministic tests cover exact-threshold arithmetic, below/above
  threshold values, zero inventory, every supported ingredient count from one
  through six, row and ingredient order, machine-only state, malformed
  parallel inputs, opaque icon pass-through, and unchanged-frame equality.
- A changed presentation frame emits one concise Debug diagnostic containing
  at most three recipe IDs, ingredient counts, and aggregate sufficient/
  insufficient counts. Invalid input emits one bounded reason per identity;
  unchanged frames emit nothing.
- Tests run without loading the shipping plugin, DSP, Unity, BepInEx, or a
  substitute runtime.

**Acceptance gate:** Focused deterministic tests and Local/Hosted Release
builds pass with zero errors. Source inspection confirms that product rules and
sufficiency arithmetic are UI-independent and have no game-state mutation,
persistence, or runtime dependency. Owner acceptance is recorded separately.

**Implementation result:**

- `RecipePresentationModel` converts ordered normalized inputs into immutable,
  structurally comparable frames while preserving opaque product and ingredient
  icon handles.
- Each valid row carries one through six ordered direct ingredients with exact
  required/current counts and `current >= required` sufficiency. Machine-only
  rows carry the supplied normalized production category; hand-craftable rows
  carry no warning.
- Malformed, unsupported, or incomplete rows return an explicit recipe-scoped
  failure and are omitted without changing the order or values of remaining
  valid rows.
- Changed frames emit one bounded Debug summary with at most three recipe IDs,
  ingredient counts, and aggregate sufficiency counts. Repeated equal frames
  are silent, and invalid identities emit one bounded reason.

**Technical validation:** Passed on 2026-08-15 with:

```powershell
$revision = git rev-parse HEAD
.\scripts\validate\Validate-S3-01.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 301 `
  -SourceRevision $revision
```

The deterministic tests cover below, exact, and above threshold arithmetic;
zero inventory; one through six ingredients including the exact six-input
recipe-75 shape; row and ingredient order; machine-only and hand-craftable
warning state; malformed parallel inputs; failure isolation; opaque icon pass-
through; structural equality; and changed-only bounded diagnostics.

Local and Hosted Release builds completed with zero warnings and zero errors.
The hosted and local products passed exhaustive compile-reference surface
coverage. Static inspection found no DSP, Unity, BepInEx, reflection, UI,
persistence, file-system, or game-state mutation dependency in the model. No
plugin was installed or executed, and no game or substitute runtime was
started.

**Owner acceptance:** Accepted explicitly on 2026-08-15. This acceptance
activated S3-02; it does not activate any later story.

### S3-02 - Bind read-only DSP recipe and inventory data

**Status:** Complete - owner accepted

**User story:** As a player, I want tracker values to come from the current DSP
recipe and Icarus inventory state without the mod changing either.

**Definition of done:**

- A narrow runtime adapter resolves each pinned ID through
  `LDB.recipes.Select(recipeId)` and reads the public `Proto.ID`,
  `RecipeProto.Items`, `RecipeProto.ItemCounts`, `RecipeProto.Handcraft`,
  `RecipeProto.madeFromString`, and `RecipeProto.iconSprite` members confirmed
  by bounded read-only assembly inspection.
- Ingredient icons resolve through the exact public chain
  `LDB.items.Select(itemId).iconSprite`. The adapter does not select a recipe
  output or use `ItemProto.maincraft`.
- A narrow inventory adapter reads each direct ingredient through
  `GameMain.mainPlayer.package.GetItemCount(itemId)` and performs no move,
  reserve, craft, factory, or save operation.
- Version-sensitive members are isolated and resolved outside repeated update
  loops where practical. The adapter returns normalized values and does not
  own pin order, sufficiency rules, Unity layout, or visibility.
- A missing recipe, inconsistent `Items`/`ItemCounts` pair, or missing required
  item/icon requests the existing safe pin-removal path, preserving the
  accepted S2-05 failure rule. A temporarily unavailable player/package or
  unexpectedly empty category value retains the pin and suppresses the
  complete affected row until valid data returns; it never invents a zero count
  or presents partial requirements.
- `RecipeProto.madeFromString` is the sole machine-warning source. Assembly IL
  confirms localized values for every Phase 1 machine-only recipe category and
  a localized Unknown default, so the MVP adds no independent category table
  or tracker-owned machine fallback.
- Product and ingredient icons are live native `iconSprite` references. No game
  art, recipe data, or dependency binary is copied or packaged.
- Focused tests with plain doubles cover complete resolution, multiple
  ingredients through the six-input baseline maximum, all recorded machine-
  only category classes, unavailable player/package state, missing required
  items/icons, unexpected empty category values, inconsistent input arrays,
  accepted invalid-pin removal, row suppression and recovery, failure
  isolation, and inert post-release behavior.
- Debug diagnostics report changed adapter availability and one bounded
  failure per recipe or item identity. They include no item names, inventory
  dump, recipe object, player object, path, or save-derived data.
- Every consumed external member is authority-backed, listed in the
  machine-readable surface inventory, and exhaustively covered by the
  declaration-only hosted shims.

**Acceptance gate:** Focused tests, Local/Hosted Release builds, exhaustive
shim validation, and static source review pass. Review confirms the exact
read-only member boundary and separation between runtime collection,
presentation rules, and Unity ownership. No runtime behavior is claimed.

**Implementation result:**

- `DspRecipeDataAdapter` refreshes the public `LDB.recipes` and `LDB.items`
  bindings once per collection, resolves only the pinned recipe and its direct
  items, and passes through live native product and ingredient icon references.
- `DspInventoryDataAdapter` resolves the current
  `GameMain.mainPlayer.package` once per collection and reads only
  `GetItemCount(itemId)` for the resolved direct ingredients.
- `RecipePresentationInputSource` converts successful reads into ordered S3-01
  inputs. It safely removes missing or structurally invalid recipe/item
  identities, retains and suppresses temporarily unavailable rows, never emits
  partial counts, and recovers without changing valid pin order.
- The adapters cache only collection-scoped runtime bindings, perform no
  reflection or mutation, and become inert after one failure-isolated release.
- Changed adapter availability and the first failure for each bounded recipe
  or item identity emit concise Debug diagnostics without runtime objects,
  item names, inventory dumps, paths, or save-derived data.
- Declaration-only shims and the consumed-surface inventory now cover every
  newly consumed public recipe, item, player, and storage member and no extra
  runtime behavior.

**Technical validation:** Passed on 2026-08-15 with:

```powershell
$revision = git rev-parse HEAD
.\scripts\validate\Validate-S3-02.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 302 `
  -SourceRevision $revision
```

The deterministic suite covers complete ordered collection, the exact six-
input recipe-75 shape, all seven Phase 1 machine-only category classes,
unavailable recipe and inventory bindings, missing recipes/items/icons,
inconsistent inputs, accepted safe removal, suppression and recovery, partial-
inventory-read rejection, exception isolation, bounded diagnostics, and inert
failure-isolated release.

Bounded read-only assembly inspection confirmed the exact public signatures.
Local and Hosted Release builds completed with zero warnings and zero errors;
both products passed exhaustive compile-reference coverage. Static review
confirmed the exact read-only member surface and found no recipe-output
selection, navigation, reflection, crafting, inventory mutation, persistence,
Unity layout, or visibility ownership. No plugin was installed or executed,
and no game or substitute runtime was started.

**Owner acceptance:** Accepted explicitly on 2026-08-15. This acceptance
activated S3-03; it does not activate any later story.

## Epic 2 - Native-composed tracker rows

### S3-03 - Render complete product and direct-ingredient rows

**Status:** Complete - owner accepted

**User story:** As a player, I want each pinned recipe shown with its native
product icon, direct ingredients, counts, sufficiency, and any machine-only
warning in one compact tracker row.

**Definition of done:**

- The existing three ordered product-icon slots become complete tracker rows
  without changing pin ordering, capacity, gesture, or visibility rules.
- Each visible row shows the pinned recipe's native product icon and every
  direct ingredient's native icon with `current / required` text for one
  unscaled operation.
- Insufficient ingredient values use restrained red treatment and sufficient
  values use restrained green treatment. Color supplements rather than
  replaces the numeric comparison.
- A machine-only recipe shows the game's normalized localized production
  category beneath the product in restrained red text. Ingredient cells do not
  show production-building labels.
- Tracker-owned UI instances use live native fonts, sprites, materials, and
  controls where available. The project adds no replacement game art or
  bespoke recipe-button prefab.
- The fixed row composition renders all one through six direct ingredients
  without silent truncation. Its maximum case is explicitly exercised with the
  Phase 1 recipe-75 shape: six distinct inputs at quantity one.
- Data outside the validated one-through-six baseline is an unsupported
  compatibility shape. It retains the pin and suppresses the affected row
  rather than showing partial requirements or clipping into another row.
- Product and ingredient representations cannot navigate, pin, unpin, craft,
  or alter inventory. They remain non-interactive throughout the MVP.
- Missing row resources disable or omit only the affected presentation;
  partial initialization and one-time tracker-owned cleanup remain safe.
- Focused tests cover three-row ordering, ingredient ordering, text values,
  sufficient/insufficient treatment, machine-only copy, missing resources,
  row composition for every ingredient count from one through six, the exact
  six-input maximum shape, unsupported larger/malformed input, failure
  isolation, cleanup, and inert release.
- Debug diagnostics record initialization/release and changed row summaries as
  bounded recipe IDs, ingredient counts, and state counts. Resource failure is
  reported once per bounded identity or resource class; unchanged frames are
  silent.
- All production external references are authority-backed and shim-covered.

**Acceptance gate:** Focused tests, Local/Hosted Release builds, exhaustive
shim validation, and source review pass. Static inspection confirms native-
resource reuse, non-interactive row behavior, fixed-panel containment, and
separation of runtime data, presentation modeling, and Unity composition.
Visual suitability remains deferred to the final owner gate.

**Implementation result:**

- `RecipeRowPresentation` converts complete S3-01 frames into ordered row
  views with invariant `current / required` text, numeric red/green treatment,
  and machine-only copy beneath the product.
- `UnityRecipeRowUiAdapter` owns three fixed rows inside the accepted tracker
  panel. Each row contains one non-raycasting native product sprite and one
  through six non-raycasting native ingredient sprites with native-font count
  text; unused cells remain inactive.
- The six-input layout remains inside the fixed 360 by 252 panel. The exact
  recipe-75 input order and every supported ingredient count from one through
  six are covered deterministically.
- Unsupported or malformed row shapes and missing product, ingredient, font,
  or host resources suppress the affected presentation without changing pin
  state. Initialization, row failures, retries, and one-time cleanup are
  isolated and bounded.
- Changed frames emit concise recipe/count and sufficiency summaries; unchanged
  frames are silent and resource failures are reported once per bounded recipe
  and resource class.
- The row composition boundary is source-ready but is not yet connected to
  live adapter collection or refresh orchestration; that connection remains
  the explicit scope of S3-04.

**Technical validation:** Passed on 2026-08-15 with:

```powershell
$revision = git rev-parse HEAD
.\scripts\validate\Validate-S3-03.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 309 `
  -SourceRevision $revision
```

The deterministic suite covers three-row and ingredient ordering, invariant
comparison text, treatment state, machine-only copy, every one-through-six
ingredient shape, exact recipe-75 inputs, containment, unsupported and
malformed shapes, missing resources, failure isolation, retry recovery,
cleanup, bounded diagnostics, unchanged-frame silence, and inert release.
Local and Hosted Release builds completed with zero warnings and zero errors;
the exact consumed `UnityEngine.UI.Text` and `UnityEngine.Font` signatures and
all declaration-only shim coverage passed. Static review found no navigation,
pinning, crafting, inventory mutation, runtime collection, reflection, or
Harmony ownership in the Unity row adapter. No plugin was installed or
executed, and no game or substitute runtime was started.

**Owner acceptance:** Accepted explicitly on 2026-08-15. This acceptance
activated S3-04; it does not activate any later story.

### S3-04 - Refresh live ingredient and inventory presentation

**Status:** Complete - owner accepted on 2026-08-15

**User story:** As a player, I want tracker counts and sufficiency to follow my
current Icarus inventory without re-pinning or disturbing pin order.

**Definition of done:**

- The orchestrator refreshes live recipe/inventory presentation while pins
  exist and applies Unity changes only when the normalized frame changes.
- Inventory changes update counts and sufficiency without changing pin order
  or requiring re-pinning. Crafting cannot remove a pin.
- Refresh work avoids broad repeated reflection scans, repeated hierarchy
  searches, unbounded diagnostics, and avoidable per-frame managed allocation.
- The existing exact six-interface rule and stored manual choice remain the
  only visibility policy. Refresh, unavailable data, and automatic hiding do
  not overwrite manual intent.
- Temporary player/package or row-resource failure suppresses only affected
  rows, retains pins, and recovers when valid data returns. Invalid recipe,
  item, input-pair, or icon evidence uses the accepted safe-removal path.
- Focused tests cover changed/unchanged refresh, inventory transitions across
  the exact threshold, pin-order stability, one-through-six ingredient frames,
  empty and unavailable states, row-level suppression/recovery, cleanup, and
  inert release.
- Debug diagnostics record only changed refresh state, row suppression/
  recovery, feature disablement, and release. Messages use bounded recipe IDs,
  counts, and booleans and contain no frame-by-frame dumps.
- All production external references are authority-backed and shim-covered.

**Acceptance gate:** Focused tests, Local/Hosted Release builds, exhaustive
shim validation, allocation/static review, and architecture review pass. A
testable DLL may be produced, but live inventory refresh and recovery remain
deferred to the final owner gate.

**Implementation result:**

- `LiveRecipePresentation` composes the accepted read-only adapters,
  presentation model, and row boundary without moving their responsibilities
  into the Unity or plugin lifecycle layers.
- Pin changes refresh immediately. Stable non-empty pin state refreshes every
  12 plugin refresh calls, avoiding recipe/inventory collection and managed
  allocation on the intervening per-frame fast path. Stable empty state is
  inert after its immediate hide operation.
- Unity rows are applied only when the normalized frame changes or when a
  previous row-resource application needs its bounded scheduled retry.
- Temporary adapter, inventory, or row-resource failure retains pins,
  suppresses affected rows, and recovers at the same cadence. Invalid recipe,
  item, input-pair, or icon evidence still uses the accepted safe-removal path
  before pin-dependent treatment and visibility refresh in the same cycle.
- The product-icon-only live path is replaced rather than duplicated. The
  complete row adapter reuses the font from the Replicator's public
  `queueCountText` and the native sprites supplied by the data adapters.
- Changed refresh state, suppression/recovery, disablement, and release emit
  bounded Debug diagnostics. Unchanged and steady-empty refreshes are silent.

**Technical validation:** Passed on 2026-08-15 with:

```powershell
$revision = git rev-parse HEAD
.\scripts\validate\Validate-S3-04.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 312 `
  -SourceRevision $revision
```

The deterministic suite covers immediate and scheduled refresh, the
allocation-free intervening fast path, unchanged-frame silence, exact
sufficiency-threshold transitions, pin-order stability, one through six direct
ingredients, complete and row-level suppression/recovery, safe invalid
removal, empty-state behavior, row retry, partial initialization, cleanup, and
inert release. Local and Hosted Release builds completed with zero warnings
and zero errors; all consumed surfaces passed exhaustive coverage. Bounded
read-only inspection confirmed the exact public
`UIReplicatorWindow.queueCountText` field and `UnityEngine.UI.Text.font` getter.
Static review confirmed no repeated
reflection or hierarchy search, no per-frame collection/allocation on the fast
path, no second visibility rule, and no crafting, inventory, factory, save, or
pin mutation outside the accepted invalid-removal path. No plugin was
installed or executed, and no game or substitute runtime was started.

**Owner acceptance:** Accepted explicitly on 2026-08-15. This acceptance
activated S3-05; it does not activate S3-06.

### S3-05 - Complete live dragging and display-bound clamping

**Status:** Complete - owner accepted on 2026-08-15

**User story:** As a player, I want to move the tracker deliberately and keep
the complete panel reachable at the supported Auto layout sizes.

**Definition of done:**

- A tracker-owned drag region supplies a real Unity `EventTriggerType.Drag`
  path through tracker-created `EventTrigger.Entry` and `TriggerEvent`
  instances. It reads `PointerEventData.delta`, preserves the panel's contained
  input behavior, and does not turn row content into an interaction surface.
- Screen-pixel drag deltas are divided by the live
  `UIRoot.instance.overlayCanvas.scaleFactor` before entering the existing UI-
  independent `DragDelta` and clamping boundary. A missing or non-positive
  scale factor disables dragging softly rather than guessing.
- The runtime adapter reads the panel parent's public `RectTransform.rect`
  width and height as the effective UI-layout bounds. It does not reproduce or
  override DSP's Auto scaling algorithm.
- Read-only assembly inspection confirms public
  `UICanvasScalerHandler.GetSuggestUILayoutHeight(int)`: Auto maps resolution
  heights 1080, 1440, and 2160 to UI layout heights 1080, 1440, and 1080
  respectively. The retained 3840-by-2160 probe independently recorded overlay
  canvas scale factor 2.
- Deterministic tests cover 1920-by-1080/Auto at effective 1920-by-1080 bounds
  and 2560-by-1440/Auto at effective 2560-by-1440 bounds. Scale conversion also
  covers the 3840-by-2160/Auto factor of 2 without making a live-behavior claim.
- Parent-bound changes re-clamp the complete fixed 360-by-252 panel. The
  contracted initial left-middle position below Goals and above the planet map
  remains the starting placement.
- Tracker-owned raycast targets contain clicks and drags without activating a
  covered HUD control or game-world action. Input containment remains for the
  final owner observation because static configuration cannot prove live event
  routing.
- Missing drag, canvas, or parent-bound members disable only dragging and
  reclamping. Partial initialization, listener removal, and one-time cleanup
  are safe and leave the rest of the tracker usable.
- Focused tests cover scale conversion, drag/clamp forwarding, both automated
  Auto cases, changed parent bounds, every edge and corner, undersized bounds,
  invalid scale/bounds, input-containment configuration, failure isolation,
  listener cleanup, and inert release.
- Debug diagnostics record initialization/release, explicit drag completion,
  changed layout bounds, clamp correction, and one bounded failure reason.
  Pointer-move events do not emit frame-by-frame log noise.
- Every consumed external member is authority-backed and shim-covered.

**Acceptance gate:** Focused tests, Local/Hosted Release builds, exhaustive
shim validation, and source review pass. Review confirms real drag-event
wiring, pixel-to-layout conversion, runtime parent-bound use, input-containment
configuration, and cleanup. Live dragging, containment, and the
3840-by-2160/Auto display case remain deferred to the final owner gate.

**Implementation result:**

- `UnityTrackerPanelDragAdapter` installs tracker-owned `Drag` and `EndDrag`
  entries on the tracker panel, translates Unity's upward-positive pointer
  delta into the panel's top-down coordinates, and removes both listeners and
  the owned trigger during cleanup.
- `TrackerPanelDrag` divides screen-pixel movement by the live overlay-canvas
  scale factor, forwards UI-layout deltas into the existing neutral geometry,
  and refreshes public parent-rectangle bounds without reproducing DSP's Auto
  scale algorithm.
- Parent-size changes re-clamp the entire fixed 360-by-252 panel. Invalid
  scale, bounds, event wiring, or drag-layout application disables only drag
  and reclamping; the panel shell, rows, visibility, and other tracker features
  remain available.
- Movement events are silent. Initialization, changed bounds, clamp
  correction, one completion per moved drag, bounded disablement, and normal
  release emit concise Debug diagnostics.
- Hosted builds now include an exact declaration-only `UnityEngine.UIModule`
  compile reference for `Canvas.scaleFactor`; the exhaustive surface inventory
  covers every new Unity/DSP member and excludes the shims from packaging.

**Technical validation:** Passed on 2026-08-15 with:

```powershell
$revision = git rev-parse HEAD
.\scripts\validate\Validate-S3-05.ps1 `
  -GameRoot '<DSP installation>' `
  -BepInExReferencePath '<documented BepInEx compile reference>' `
  -BuildNumber 305 `
  -SourceRevision $revision
```

The deterministic suite covers factor-two scale conversion, 1920-by-1080 and
2560-by-1440 Auto effective bounds, drag forwarding, every edge and corner,
changed and undersized bounds, invalid scale and bounds, tracker-owned raycast
configuration, failure isolation, listener cleanup, and inert release. Local
and Hosted Release builds completed with zero warnings and zero errors, and
their consumed external surfaces matched under exhaustive shim validation.
Read-only inspection confirmed the public `UIRoot.overlayCanvas`,
`Canvas.scaleFactor`, `RectTransform.rect`, `Rect.width`/`height`,
`PointerEventData.delta`, `EventTriggerType.Drag`/`EndDrag`, and
`UICanvasScalerHandler.GetSuggestUILayoutHeight(int)` members. Static review
confirmed no duplicate Auto-scale algorithm, broad reflection or hierarchy
search, per-pointer diagnostic stream, row interaction surface, or game-state
mutation. No plugin was installed or executed, and no game or substitute
runtime was started.

**Owner acceptance:** Accepted explicitly on 2026-08-15. This acceptance
activated S3-06; it does not complete Sprint 3 or satisfy a sprint exit gate.

## Epic 3 - Complete and hand off the MVP prototype

### S3-06 - Complete orchestration and prepare the owner checkpoint

**Status:** Active - implementation complete; exact-revision technical
validation pending

**User story:** As the owner, I want one coherent testable prototype and a
small, complete procedure for judging only the remaining live behavior.

**Definition of done:**

- Plugin startup composes the accepted state, input, treatment, visibility,
  recipe/inventory, presentation, row, and panel boundaries without moving
  product rules into Unity or lifecycle code.
- Partial initialization is feature-isolated. Shutdown removes every tracker-
  owned listener and releases every tracker-owned object, material, buffer, and
  other resource exactly once; callbacks are inert afterward.
- The complete deterministic story-validation chain, Local/Hosted Release
  builds, exhaustive compile-reference coverage, architecture checks, and
  static package inspection pass for one recorded source revision.
- A real version-aligned test DLL and Thunderstore-layout package are produced
  from that revision. Shims, dependencies, PDBs, generated evidence, saves,
  logs, screenshots, secrets, and player-specific paths are absent.
- README and contract status describe implemented versus owner-observed
  behavior truthfully. Navigation remains excluded and display claims remain
  within the selected validation boundary; no publication or compatibility
  claim is inferred.
- One self-contained owner procedure identifies the exact DLL/package, purpose,
  prerequisites, safe setup, actions, expected observations, cleanup, and a
  simple pass/fail/unexpected-result report format.
- The procedure groups only the otherwise-unprovable live checks: native
  right-click coexistence and cell treatment; row composition and live counts;
  machine-only presentation; dragging and containment; exact six-interface and
  paired manual visibility; lifecycle/cleanup symptoms; and the single
  3840-by-2160/Auto display case at a 1080-pixel UI layout height.
- The procedure assumes no project knowledge, avoids repetitive recipes and
  broad matrices, and does not ask the owner to re-prove deterministic ordering,
  arithmetic, package, or shim facts. Logs or screenshots are requested only
  to diagnose a specific unexpected observation.
- Startup, feature availability, material state changes, manual actions,
  presentation changes, isolated failures, and shutdown have concise bounded
  Debug diagnostics sufficient to interpret the procedure without routine
  data dumps.

**Acceptance gate:** The complete non-runtime validation chain passes and the
testable artifact, evidence summary, known limitations, and proposed human
procedure are ready for owner review. Agents do not run the procedure.

**Implementation result:**

- Plugin startup composes every accepted Sprint 2 and Sprint 3 boundary through
  `TrackerOrchestrator`; the orchestrator retains only lifecycle, refresh, and
  visibility coordination.
- Deterministic integration coverage now isolates panel, drag, input,
  treatment, presentation, and visibility-control initialization failures one
  at a time. Every case verifies exact availability reporting, one-time
  cleanup, and inert input callbacks after release.
- Cleanup remains ordered from controls and drag listeners through row/data
  presentation, treatment, pin input, and finally the panel shell so owned
  children and callbacks are released before their host.
- `Validate-S3-06.ps1` closes the story-level non-runtime chain.
  `Validate-Sprint3-SourceReady.ps1` and
  `Validate-Sprint3-PackageInspected.ps1` provide distinct exact-revision exit
  gates without performing or inferring live validation.
- The self-contained owner procedure limits future runtime work to one
  3840-by-2160/Auto session and the remaining native integration, presentation,
  input, visibility, and lifecycle observations.

**Owner procedure:** [`Sprint 3 Owner Validation Procedure`](OWNER-VALIDATION.md)

## Sequence and dependencies

```text
S3-01 presentation model
    |
    `--> S3-02 read-only DSP adapters
             |
             `--> S3-03 complete native-composed rows
                      |
                      `--> S3-04 live presentation refresh
                               |
                               `--> S3-05 dragging and display clamping

S3-01 through S3-05
    |
    `--> S3-06 final orchestration and exit handoff
```

Stories are activated one at a time. Plain deterministic rules precede the
version-sensitive runtime boundary, and full row composition precedes refresh
orchestration. Live data refresh precedes the separate drag, scale, and bounds
integration. Tracker-icon navigation remains outside the MVP.

## Sprint exit gates

### Source-ready

**Status:** Pending implementation

- Every included Sprint 3 story meets its definition of done. S3-01 through
  S3-05 are explicitly owner accepted; S3-06 is the technical handoff into the
  remaining sprint exit gates.
- The complete focused validation chain and Local/Hosted Release builds pass
  with zero errors for one recorded source revision.
- Every consumed external reference is authority-backed and exhaustively
  covered by declaration-only shims with no extra surface.
- Static review confirms read-only game state, deterministic arithmetic,
  architecture separation, bounded refresh work and diagnostics, failure
  isolation, input-containment configuration, and complete owned-resource
  cleanup paths.
- Documentation distinguishes deterministic evidence from unknown live
  behavior, excludes tracker navigation, and limits display claims to the
  recorded automated and owner-performed cases.

### Package-inspected

**Status:** Pending implementation

- The real version-aligned Release output passes Local and Hosted static
  package inspection.
- The archive contains only the package-contract files and the intended real
  plugin assembly under the exact install path.
- No shim, dependency binary, PDB, build input, generated evidence, save, log,
  screenshot, secret, or player-specific path enters the archive.
- Metadata truthfully describes the development artifact and makes no
  unsupported installed, compatibility, publication, or release claim.

### Behavioral-ready

**Status:** Pending owner-performed validation

- Source-ready and Package-inspected pass first.
- The owner runs the supplied testable build using the bounded procedure and
  reports each grouped observation as pass, fail, or unexpected.
- Only claims that require live DSP judgment are decided here. Automated facts
  are not repeated as human workload.
- Any failed or unexpected observation returns only the affected scope for
  diagnosis and repair; it is not silently waived or generalized.

### Owner-reviewed

**Status:** Pending explicit owner acceptance

- The owner reviews the complete technical evidence, owner-performed results,
  deferred claims, and known limitations.
- The owner explicitly accepts or rejects the Sprint 3 exit.
- Owner review does not imply publication approval, compatibility beyond the
  observed boundary, or a supported release.

## Explicitly unavailable states

Until the applicable gates pass, Sprint 3 establishes no `Behavioral-ready`,
installed, visual, interaction, cleanup, compatibility, general display-
support, `Publication-ready`, or supported-release status. Source-ready and
Package-inspected evidence cannot establish those states by inference.

Even after a successful bounded owner procedure, observations apply only to
the tested build and selected conditions. Publication remains a separate owner
decision under `VALIDATION-CONTRACT.md` and `THUNDERSTORE-PACKAGE.md`.

## Decisions and change control

Tracker-icon navigation is excluded from the MVP. Automated display validation
is limited to 1920-by-1080/Auto and 2560-by-1440/Auto; owner-performed runtime
validation is limited to 3840-by-2160/Auto at a calculated 1080-pixel UI layout
height. Broadening either boundary requires an explicit owner-approved change.

An owner-approved roadmap change is required to alter pin behavior, eligibility,
ordering, capacity, lifetime, direct-ingredient or Icarus-only scope, machine-
only presentation, the exact visibility rule, player-owned visibility,
read-only game-state behavior, native-resource reuse, runtime-validation
boundary, or a deferred feature.
