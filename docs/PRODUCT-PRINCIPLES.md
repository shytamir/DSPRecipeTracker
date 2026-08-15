# DSP Recipe Tracker - Product Principles

## Status and authority

These principles define the current product character of DSP Recipe Tracker.
They refine the original concept after runtime feasibility work against the
supported Dyson Sphere Program build. `docs/PROJECT.md` turns them into the
MVP contract; the original concept record remains non-authoritative history.

If an implementation choice conflicts with these principles, prefer the
smaller extension of the game's existing interface unless the owner explicitly
changes the product direction.

## 1. Extend the game instead of replacing it

DSP Recipe Tracker is a presentation extension over the Replicator. It does
not introduce a parallel recipe browser, unlock model, crafting simulation, or
inventory system.

The game remains authoritative for:

- which recipes the Replicator presents and permits the player to use;
- recipe selection and ordinary Replicator interaction;
- recipe inputs, outputs, icons, and production category;
- technology unlock state;
- Icarus inventory quantities; and
- native UI assets and feedback.

The mod reads and presents that state. It does not reinterpret it as new game
rules.

## 2. Keep mod-owned logic narrow

The mod owns only UI orchestration and transient tracker state:

- the ordered list of at most three pinned recipe identities;
- pin toggle and FILO eviction behavior;
- direct-ingredient and inventory values prepared for display;
- tracker layout, dragging, input capture, and visibility; and
- the global Show/Hide control integrated into the game's own HUD group.

It owns no independent recipe, unlock, crafting, factory, or save-state
behavior. Tracking remains read-only, and pin state lasts only for the current
play session in the MVP.

## 3. Pin only through native Replicator recipe controls

The Replicator recipe grid is the sole valid pinning surface. The tracker
panel, inventory, technology screen, and other game surfaces must not create
pins.

Pinning is added to the native recipe-control interaction through right-click.
Left-click remains selection-only. Right-click follows the game's existing
selection path and then toggles the pin, so native behavior still runs
unchanged and ordinary recipe selection cannot accidentally change tracker
state.

The recipe-control presentation communicates transient pinned state with a
restrained green corner-bracket treatment on at most three pinned recipes.
Available, unpinned recipes remain visually neutral. The tracker-owned markers
are non-raycasting and must not conceal or
replace recipe artwork or the game's hover, selection, disabled, machine-only,
or other native states. Tracker state never enters the Replicator's original
state buffer.

The original principle called for full-cell green filtering when available and
red filtering when pinned. Live prototype evidence showed that treatment
obscured icons and could become excessively bright, while distinguishing every
available cell offered little additional value. Pinned-only corners communicate
the same actionable state immediately, with no extra cognitive step and much
lower visual cost. The owner accepted that evidence-driven deviation; a future
available-state treatment requires a new design rather than restoration by
default.

## 4. Inherit eligibility and technology lock

The mod does not maintain a second technology-lock calculation for pinning.
Only a recipe cell that the supported Replicator has populated and made
interactive can initiate a pin action. The Replicator's own population and
interaction logic therefore supplies the eligibility boundary.

Runtime unlock APIs may be used for diagnostics and fail-soft validation, but
they must not broaden the set of recipes exposed by the native Replicator.

## 5. Reuse native presentation

Tracker recipe presentation derives from the Replicator's recipe-cell visual
language and reuses live game-provided icons, sprites, fonts, materials, and
sounds. The project does not ship replacement game art or a bespoke recipe
button prefab.

In the supported build, Replicator recipe cells are GPU-rendered entries in a
shared grid rather than individual cloneable Unity `Button` objects. "Clone"
therefore means reproducing the native recipe-cell composition from existing
runtime resources inside tracker-owned UI instances; it does not mean that a
per-recipe GameObject exists to instantiate. The panel and its runtime
instances are new UI objects, but their recipe presentation introduces no new
asset vocabulary.

Tracker recipe representations are not pinning surfaces. Any later navigation
behavior must use the game's own Replicator navigation and must not turn those
representations into a second pinning interface.

## 6. Inherit native visibility where possible

The global Show/Hide control belongs to `UIGame.gameMenu` and inherits that
HUD group's visibility and lifecycle. The tracker panel adds only the explicit
visibility rules required by the product:

- hidden when empty;
- hidden when the player chooses Hide; and
- hidden while Tech, Dyson Editor, Inventory, Replicator, Statistics, or
  Dashboard is active.

The mod must not invent a broader meaning of "major UI" from unrelated game
windows.

## 7. Fail softly at the game boundary

Game members, recipe identities, assets, and UI hosts are version-sensitive.
If expected runtime evidence is unavailable, the affected integration must
skip, hide, or discard invalid presentation safely and log a bounded
diagnostic. It must not corrupt tracker order, block the Replicator, or alter
game state.

Compatibility work begins from observed conflict evidence. The project does
not carry speculative adapters for untested game, loader, or UI-mod versions.
