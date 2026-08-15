# DSP Recipe Tracker - Project Contract

## Status and authority

**Status:** Current product contract

**Product:** DSP Recipe Tracker

**BepInEx plugin GUID:** `dsprecipetracker`

**BepInEx display name:** `DSP-Recipe-Tracker`

**Implementation status:** The bootstrap, native-integration, and prototype
roadmaps are complete and owner accepted. Sprint 3 produced a working validated
MVP prototype. The owner passed all four Behavioral-ready groups on 2026-08-15
and accepted final direct workshop build `0.1.318`. Source-ready and Package-
inspected subsequently passed for clean source revision
`14bbe8e046e32333bd7cf68f35b8f22bc04dd47f` using build number 318. The source
contains the complete right-click pinning, transient three-entry state,
pinned-corner treatment, direct-ingredient and Icarus-inventory presentation,
live refresh, exact six-interface visibility, paired controls, borderless
fixed panel, scale-aware dragging and clamping, and game-session cleanup path.
The repository is planning-pending for publication refinement. No supported or
published release exists.

**Owner review:** Accepted through 2026-08-15.

This file is the normative authority for settled product behavior and scope.
It consolidates the product direction in
[`PRODUCT-PRINCIPLES.md`](PRODUCT-PRINCIPLES.md), the confirmed integration
surface in [`FEASIBILITY.md`](FEASIBILITY.md), and the original intent retained
in [`planning/PINNED-RECIPE-TRACKER-CONCEPT.md`](planning/PINNED-RECIPE-TRACKER-CONCEPT.md).

When those sources differ, this contract governs product behavior. The
feasibility record remains authoritative for the runtime facts it proves. The
linked implementation and validation contracts govern how this product is
built and how claims are established; they may not change product behavior.
[`ROADMAP.md`](ROADMAP.md) alone governs active implementation scope and
operational authorization.

This contract does not:

- activate a roadmap story;
- record technical validation or owner acceptance;
- authorize installation, game execution, or runtime evidence collection;
- approve a release or publication; or
- promote a deferred feature into the MVP.

## 1. Product intent

DSP Recipe Tracker is a restrained presentation extension over Dyson Sphere
Program's Replicator. It lets the player pin a small number of recipes and see
whether Icarus currently carries each recipe's direct ingredients.

The product reduces repeated Replicator visits while the player gathers or
automates components. It is an information aid, not a recipe browser,
inventory system, crafting simulation, or factory automation tool.

DSP remains authoritative for:

- recipe population, selection, unlock state, inputs, outputs, and production
  category;
- ordinary Replicator interaction and feedback;
- recipe and item icons and other native presentation resources;
- Icarus inventory quantities; and
- crafting, factory, and save state.

The mod reads and presents that state. It must not reinterpret or modify it as
a new game rule.

## 2. MVP product behavior

### 2.1 Pinning surface and gesture

- The native Replicator recipe grid is the only pinning surface.
- No separate Pin or Unpin button is added.
- Left-click remains native selection-only.
- Right-click follows native recipe selection and then toggles the identified
  recipe's pin.
- Right-clicking an already pinned recipe unpins it.
- Tracker rows, inventory controls, technology controls, and other game
  surfaces cannot create or remove pins.

### 2.2 Eligibility

- Only a recipe cell populated and made interactive by the native Replicator
  may initiate a pin.
- This inherits the game's technology lock and Replicator eligibility rules
  instead of maintaining a second unlock model.
- All item recipes admitted by that native boundary, including building and
  machine-only recipes, are eligible.

### 2.3 Pin order and lifetime

- Tracker state is an ordered, transient list of at most three recipe
  identities.
- A new pin becomes the first, top entry.
- Existing entries retain their relative order beneath it.
- When three recipes are pinned, a fourth pin evicts the third, bottom entry.
- Crafting a pinned recipe does not remove its pin.
- A pin leaves the tracker only through explicit unpinning, FILO eviction, or
  safe removal when its recipe is no longer available.
- Pins, manual visibility, and panel position are session state only in the
  MVP. They are not written to save data or restored across sessions.

### 2.4 Material comparison

- The tracker reads Icarus's inventory only.
- Storage, logistics stations, factories, the wider planet, and other players
  are not counted.
- Each row shows the pinned recipe's direct ingredients only. Ingredient
  recipes are not recursively expanded.
- Requirements represent one unscaled recipe operation.
- Every ingredient shows `amount in inventory / amount required`.
- Inventory below the exact requirement is insufficient and shown in red.
- Inventory equal to or above the exact requirement is sufficient and shown
  in green.
- Inventory changes update presentation without changing pin order or
  requiring the player to recreate the pin.

### 2.5 Machine-only recipes

- A recipe that cannot be hand-crafted remains pinnable when the Replicator
  admits it.
- Its row displays the game's localized production category or required
  facility in a dedicated full-width footer using restrained amber text.
- Ingredient icons do not recursively display production-building labels.
- The tracker does not invoke or simulate machine production.

### 2.6 Native recipe-cell treatment

- An available, unpinned native recipe cell receives no tracker treatment.
- A pinned native recipe cell receives a restrained green corner-bracket
  treatment; at most three pinned cells are marked.
- The markers are tracker-owned and non-raycasting. They preserve recipe art
  and native hover, selection, disabled, machine-only, and other game states.
- Tracker treatment remains independent from DSP's original recipe-grid state;
  tracker state never overwrites native state.

The original MVP promise specified full-cell green treatment for available
recipes and red treatment for pinned recipes. The first live implementation
proved deleterious: full coverage obscured recipe artwork, produced extreme
brightness on some icons, and added presentation cost without proportional
value. The owner accepted pinned-only corner markers because they preserve the
proven utility: pin state remains immediately recognizable without additional
cognitive load. A distinct available-recipe treatment is deferred rather than
simulated poorly.

### 2.7 Tracker panel

- The tracker is a fixed-size panel for the MVP.
- Its fixed size is 360 by 300 UI-layout units.
- Its default position is derived from the live layout height as true left-
  middle placement, keeping it clear of the top-left HUD at the validated
  layouts.
- It presents no more than three ordered recipe rows.
- Each row uses the pinned product icon, direct-ingredient icons, quantities,
  sufficiency state, and any applicable machine-only warning.
- `TARGET` and `INGREDIENTS` headings and a restrained separator make the row
  roles explicit. Ingredient quantities are centered beneath their icons.
- The panel uses a flat dark translucent background without a border.
- Presentation uses the game's existing visual language and live native
  resources. The project ships no replacement game art or bespoke recipe-
  button prefab.
- The player may drag the panel by its designated drag region.
- The complete panel remains reachable within the screen after dragging or a
  display-size change.
- Panel interaction must not activate the game world or a covered HUD control.
- The panel is not another pinning surface.

The panel was increased from 360 by 252 to 360 by 300 UI-layout units after
several direct-build attempts could not make native multiline facility text
wrap reliably. A dedicated single-line footer ended that unproductive cycle
while preserving complete facility names and three contained rows. The owner
accepted this bounded presentation compromise; panel height may be reviewed in
future publication refinement, while interactive resizing remains outside the
MVP.

### 2.8 Visibility and player control

One deterministic policy owns tracker-panel visibility:

```text
visible = hasRows && manualRequested && !majorInterfaceActive
```

- `hasRows` is true when at least one valid recipe remains pinned.
- `manualRequested` is controlled only by the player's tracker Hide and global
  Show/Hide actions. Automatic conditions do not overwrite that choice.
- `majorInterfaceActive` is the OR of exactly Tech, Dyson Editor, Inventory,
  Replicator, Statistics, and Dashboard.
- Unrelated machine and function windows do not expand this rule by inference.
- The panel hides when empty, when manually hidden, or while one of the six
  major interfaces is active.
- A Hide control belongs to the panel.
- A global Show/Hide control belongs to the existing bottom-right game-menu
  group, inherits that group's native visibility, and occupies the validated
  non-colliding position above its screenshot-button reference.

### 2.9 Tracker-icon navigation

Navigation from product or ingredient icons to the corresponding Replicator
recipe has a confirmed native integration path, but tracker navigation is
excluded from the MVP. Product and ingredient icons remain non-interactive
presentation and cannot navigate, pin, or unpin.

The confirmed native path remains feasibility evidence for possible later
work. Promoting navigation requires an explicit post-MVP product and roadmap
change with focused interaction and identity criteria.

## 3. Product safety and failure behavior

- DSP, Icarus inventory, crafting, factory, and save state are read-only.
- The mod does not craft, replicate, move items, place buildings, or write
  product data to a save.
- A missing recipe, inconsistent direct-input data, or missing required
  item/icon removes only that invalid pin while preserving remaining order.
- Temporary player/package, production-category, presentation-resource, host,
  or integration unavailability retains pins and suppresses only affected rows
  until recovery.
- Failure never presents partial or invented requirements, corrupts remaining
  pin order, or blocks the Replicator.
- Compatibility claims begin from observed evidence. The MVP promises no
  speculative support for untested game, loader, or UI-mod versions.

## 4. Linked delivery contracts

The following contracts implement and validate this product:

| Contract | Responsibility |
| --- | --- |
| [`IMPLEMENTATION-CONTRACT.md`](IMPLEMENTATION-CONTRACT.md) | Architecture, runtime adapters, native integration mechanics, state ownership, failure cleanup, and source baseline. |
| [`VALIDATION-CONTRACT.md`](VALIDATION-CONTRACT.md) | Source, package, behavioral, owner-acceptance, and publication gates; evidence and runtime-authorization rules. |
| [`THUNDERSTORE-PACKAGE.md`](THUNDERSTORE-PACKAGE.md) | Package layout, version mapping, build-reference boundary, artifact validation, and publication boundary. |
| [`RUNTIME-AUTHORITY.md`](RUNTIME-AUTHORITY.md) | Runtime authority hierarchy, consumed-surface validation, and evidence boundaries. |
| [`BEPINEX-CONFORMANCE.md`](BEPINEX-CONFORMANCE.md) | Supported loader version and permitted BepInEx feature surface. |
| [`ROADMAP.md`](ROADMAP.md) | Current planning state and implementation authorization, when any. |

Anyone changing implementation must read the implementation contract and the
active roadmap. Anyone making a readiness, acceptance, compatibility, release,
or publication claim must read the validation contract.

No linked contract may broaden or weaken the product rules in this file. A
conflict is resolved in favor of `PROJECT.md` unless the owner explicitly
changes the product contract.

## 5. Deferred features

The following remain outside the MVP until the owner explicitly promotes one
with scope and acceptance criteria:

- per-recipe target quantity and ingredient scaling;
- saved pins or panel state;
- automatic hand-crafting or replication;
- transitions and animation;
- panel resizing;
- broader inventory scopes;
- recursive ingredient expansion;
- broader localization tables;
- speculative compatibility layers; and
- tracker-icon navigation, as described in section 2.9.

If automatic replication is promoted, it must default off, apply only to
recipes the game considers hand-craftable, and use native crafting rules.
Machine-only recipes remain tracking-only.

If persistence is promoted, its save/profile/global boundary and stable recipe
fingerprint require an explicit owner decision.

## 6. Explicit non-goals

- A parallel recipe browser or independent technology-lock model.
- A second pinning surface in the tracker or elsewhere.
- Tracking storage, logistics networks, planets, or multiplayer inventories.
- Recursive raw-material planning.
- Automatic unpinning after crafting.
- Inventory movement, crafting automation, factory control, or building
  placement.
- Custom replacement art, sound packs, or elaborate effects.
- Multi-loader or speculative multi-version compatibility.
- Automatic miner placement, geothermal placement, or other unrelated mod
  concepts retained in the historical discussion.

## 7. MVP presentation validation boundary

- Tracker-icon navigation is excluded from the MVP.
- Deterministic geometry and scale validation covers 1920-by-1080 at Auto UI
  layout height 1080 and 2560-by-1440 at Auto UI layout height 1440.
- Owner-performed runtime display validation covers only 3840-by-2160 with
  Auto scale, which calculates to a 1080-pixel UI layout height.
- The MVP makes no resolution or UI-scale support claim beyond those explicit
  automated and owner-performed boundaries.

No implementation agent may broaden these boundaries from installed plugins,
historical probes, placeholder metadata, or personal preference.

## 8. Product change control

An explicit owner decision is required to:

- alter pin eligibility, gesture, ordering, capacity, or lifetime;
- broaden inventory or recipe scope;
- add persistence, crafting, automation, navigation, or another interaction
  surface;
- change the six-interface visibility rule or player-owned visibility model;
- weaken read-only game-state behavior or independent cell treatment;
- promote a deferred feature; or
- claim a new supported runtime or compatibility boundary.

Update this file and public documentation together when product behavior
changes. Roadmap status, technical validation, owner acceptance, packaging,
and publication remain separate records.
