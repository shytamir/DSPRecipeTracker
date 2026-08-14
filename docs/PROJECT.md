# DSP Recipe Tracker - Project Contract

## Status and authority

**Status:** Current product contract

**Product:** DSP Recipe Tracker

**BepInEx plugin GUID:** `dsprecipetracker`

**BepInEx display name:** `DSP-Recipe-Tracker`

**Implementation status:** The bootstrap roadmap and S1-01 through S1-06 are
complete and owner accepted. Its Source-ready, Package-inspected, and
Owner-reviewed exit gates passed. The owner-authorized Sprint 2 roadmap is
active, implementation is under way, S2-01 through S2-03 are owner accepted,
and S2-04 is implemented and technically validated pending owner acceptance.
The source contains the minimal plugin
skeleton, inspected package pipeline, UI-independent panel geometry and
visibility policy, inert compile-time Unity panel boundary, and deterministic
transient pin state, an isolated native Replicator input adapter, and an
independent native recipe-grid treatment adapter, and the exact fail-closed
six-interface visibility input. They are not yet connected to plugin startup;
no supported release exists.

**Owner review:** Accepted on 2026-08-14.

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
  facility beneath the pinned product in restrained red text.
- Ingredient icons do not recursively display production-building labels.
- The tracker does not invoke or simulate machine production.

### 2.6 Native recipe-cell treatment

- An available, unpinned native recipe cell receives a restrained green
  treatment.
- A pinned native recipe cell receives a restrained red treatment.
- The treatment preserves native hover, selection, disabled, machine-only, and
  other game states.
- Tracker treatment remains independent from DSP's original recipe-grid state;
  tracker state must not overwrite native state.

### 2.7 Tracker panel

- The tracker is a fixed-size panel for the MVP.
- Its default position is left-middle, below Goals and above the planet map.
- It presents no more than three ordered recipe rows.
- Each row uses the pinned product icon, direct-ingredient icons, quantities,
  sufficiency state, and any applicable machine-only warning.
- Presentation uses the game's existing visual language and live native
  resources. The project ships no replacement game art or bespoke recipe-
  button prefab.
- The player may drag the panel by its designated drag region.
- The complete panel remains reachable within the screen after dragging or a
  display-size change.
- Panel interaction must not activate the game world or a covered HUD control.
- The panel is not another pinning surface.

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
  group and inherits that group's native visibility.

### 2.9 Tracker-icon navigation

Navigation from product or ingredient icons to the corresponding Replicator
recipe has a confirmed native integration path, but it is not part of the
current MVP baseline. It remains decision-gated for the presentation sprint.

If the owner promotes navigation:

- exact pinned-recipe identity is preserved when products have multiple
  recipes;
- the game's native Replicator navigation and unlock behavior remain
  authoritative;
- tracker representations may navigate but may not pin or unpin; and
- navigation receives focused acceptance criteria.

Excluding navigation does not block the core tracker MVP and must be reflected
in public documentation and package metadata.

## 3. Product safety and failure behavior

- DSP, Icarus inventory, crafting, factory, and save state are read-only.
- The mod does not craft, replicate, move items, place buildings, or write
  product data to a save.
- Missing recipes, icons, resources, or game integration fail softly.
- An unavailable row or presentation is skipped or removed without corrupting
  remaining pin order or blocking the Replicator.
- Compatibility claims begin from observed evidence. The MVP promises no
  speculative support for untested game, loader, or UI-mod versions.

## 4. Linked delivery contracts

The following contracts implement and validate this product:

| Contract | Responsibility |
| --- | --- |
| [`IMPLEMENTATION-CONTRACT.md`](IMPLEMENTATION-CONTRACT.md) | Architecture, runtime adapters, native integration mechanics, state ownership, failure cleanup, and source baseline. |
| [`VALIDATION-CONTRACT.md`](VALIDATION-CONTRACT.md) | Source, package, behavioral, owner-acceptance, and publication gates; evidence and runtime-authorization rules. |
| [`THUNDERSTORE-PACKAGE.md`](THUNDERSTORE-PACKAGE.md) | Package layout, version mapping, build-reference boundary, artifact validation, and publication boundary. |
| [`RUNTIME-AUTHORITY.md`](RUNTIME-AUTHORITY.md) | Runtime evidence identity, provenance, coverage, and refresh rules. |
| [`BEPINEX-CONFORMANCE.md`](BEPINEX-CONFORMANCE.md) | Supported loader version and permitted BepInEx feature surface. |
| [`ROADMAP.md`](ROADMAP.md) | Active story, implementation scope, sequencing, and operational authorization. |

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

## 7. Open product decisions and entry gates

The following are not decided by this contract:

- inclusion or exclusion of tracker-icon navigation;
- supported resolution and UI-scale matrix.

These decisions must be recorded in the applicable roadmap story before that
story becomes Ready. No implementation agent may invent them from installed
plugins, historical probes, placeholder metadata, or personal preference.

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
