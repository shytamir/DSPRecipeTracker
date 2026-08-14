# DSP Recipe Tracker - Feasibility Findings

## Status

This record answers the initial design and technical questions using the
authority snapshot and installed runtime available on 2026-08-14. It separates
confirmed game behavior from implementation recommendations and validation
that still requires an initialized in-game checkpoint.

These findings constrain implementation but do not replace the product
principles or owner acceptance.

## Evidence and method

The investigation used:

- `Assembly-CSharp.dll` SHA-256
  `ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85`;
- the validation-passing Phase 1 recipe export linked to that exact assembly;
- direct metadata and IL inspection with the installed Mono.Cecil assembly;
- the installed DSP `0.10.34.28529`, Steam build `23109513`;
- Unity `2022.3.62f3c1` (`2022.3.62.1451004` file version); and
- BepInEx `5.4.17.0` with HarmonyX `2.5.5.0`.

The runtime probes reused the isolated BepInEx harness pattern from the local
DSPSeedScanner project: installed game data was exposed to a disposable
runtime, user plugins were excluded, one probe plugin ran, results were
written once, and the game exited automatically. The original three hierarchy
launches and the focused gesture/material follow-up launches exited with code
0. No save was opened or modified.

The retained local probe source and its 1,565-record result are outside the
tracked repository at:

```text
D:\Shy\Shared Untracked Repo Resources\DSPRecipeTracker-FeasibilityProbe\
```

The UI hierarchy was inspected at the main menu. Relevant objects were
created but not initialized for a loaded game, so this record does not claim
completed in-game placement, input, or display-scale validation.

The final focused treatment result is retained as `pin-treatment-final.tsv`
with SHA-256
`de861e56bb4e2c01cd7b64d07eb44c75fb94735b7d4414f3ff49f7225965909e`.
Its graphical render is `pin-treatment-final.png` with SHA-256
`07aec87ca5d9d94225984dc859bb215197bbc9113da7bf8f625e76a02084279`.

## Resolved questions

### 1. Replicator pinning surface

The product decision supersedes the earlier proposal for a separate
Pin/Unpin button: the Replicator recipe grid is the sole pinning surface.

In the supported assembly, recipes are not represented by individual Unity
buttons. `UIReplicatorWindow` renders a 14-by-8 cell grid through shared
`recipeIcons`, `recipeBg`, materials, compute buffers, and the parallel
`recipeProtoArray`, `recipeIndexArray`, and `recipeStateArray`. A shared
`EventTrigger` named `evtRecipe` routes pointer input to
`OnRecipeMouseDown(BaseEventData)` after `TestMouseRecipeIndex()` identifies
the cell.

`RefreshRecipeIcons()` populates only recipes accepted by the game's own
unlock and grid rules. It calls `GameHistoryData.RecipeUnlocked(recipe.ID)`
and records machine-only state from `RecipeProto.Handcraft`. This confirms
that native population can remain the pin-eligibility boundary.

The shared `EventTrigger` has `PointerDown`, `PointerEnter`, and `PointerExit`
entries. The installed Unity `StandaloneInputModule` sends left, right, and
middle mouse buttons through `ProcessMousePress`, while DSP's private
`OnRecipeMouseDown(BaseEventData evtData)` does not inspect `evtData` and
therefore preserves selection for right-click.

The adopted pin gesture is right-click. Append a listener to the existing
`PointerDown` callback after the Replicator is created, filter for
`PointerEventData.InputButton.Right`, and toggle the recipe identified by the
same grid index and recipe array used by DSP. Left-click remains
selection-only; right-click selects and toggles. Remove the listener during
plugin shutdown.

This existing event is sufficient; the MVP pin gesture does not require a
Harmony patch. The version-sensitive recipe array and mouse index bindings
should be resolved once and cached rather than reflected in the update loop.

### 2. Navigation to a recipe

`UIGame.FocusOnReplicate(int itemId)` is the public native path for item-based
navigation. It selects the item through `LDB.items`, opens the Replicator and
inventory if needed, and selects `ItemProto.maincraft`.

When exact recipe identity matters, use `UIGame.OpenReplicatorWindow()` and
`UIReplicatorWindow.SetSelectedRecipe(RecipeProto, true)`. The latter uses the
recipe's grid position and applies the game's own unlock check. This avoids
substituting an item's main recipe when a pinned product has multiple recipes.

Navigation requires no new Harmony patch. Whether tracker representations are
interactive remains deferred to feature development; they must never become
another pinning surface.

### 3. Authoritative recipe and inventory members

| Concern | Confirmed member |
| --- | --- |
| Recipe identity | `Proto.ID` inherited by `RecipeProto` |
| Runtime lookup | `LDB.recipes.Select(recipeId)` |
| Unlock query | `GameMain.history.RecipeUnlocked(recipeId)` |
| Direct inputs | `RecipeProto.Items` and `RecipeProto.ItemCounts` |
| Outputs | `RecipeProto.Results` and `RecipeProto.ResultCounts` |
| Recipe category | `RecipeProto.Type` |
| Hand craftability | `RecipeProto.Handcraft` |
| Production category label | `RecipeProto.madeFromString` |
| Icarus inventory | `GameMain.mainPlayer.package.GetItemCount(itemId)` |
| Recipe and item art | `RecipeProto.iconSprite` and `ItemProto.iconSprite` |

`RecipeProto.madeFromString` maps the game's recipe categories to localized
facility labels and is preferred over the sparse `ItemProto.produceFrom`
field.

### 4. Major-interface visibility

The agreed major interfaces are directly available from `UIGame` and inherit
`ManualBehaviour`:

| Interface | Active-state source |
| --- | --- |
| Tech | `techTree.active` |
| Dyson Editor | `dysonEditor.active` |
| Inventory | `inventoryWindow.active` |
| Replicator | `replicator.active` |
| Statistics | `statWindow.active` |
| Dashboard | `dashboard.active` |

A centralized OR of these six flags exactly represents the product rule.
`UIGame.isAnyFunctionWindowActive` is unsuitable because it includes unrelated
machine windows while omitting members of the agreed set.
`UIGame.uiPanelActiveMask` is also insufficient because it has no explicit
Inventory bit. Reading six booleans during UI refresh is the narrowest robust
implementation and does not justify six lifecycle patches.

### 5. Global Show/Hide integration

The native control group is `UIGame.gameMenu`, a `ManualBehaviour` under:

```text
UI Root/Overlay Canvas/In Game/Game Menu
```

Adding the control as a child of this group makes it inherit native group
visibility. The smallest existing template is `buttonS`, the 46-by-46
screenshot control; the main game-menu controls are 70-by-70.

The inspected group is anchored at the **bottom-right**, correcting the
earlier bottom-left description. The icon asset remains a feature-development
choice. A runtime clone must clear inherited listeners, tooltip data, and
localization behavior before receiving tracker behavior.

### 6. Native presentation resources

The runtime contains reusable UI resources including:

- fonts such as `SAIRAB`, `SAIRASB`, `VERDANAB`, `DIN`, and `AGENCYR`;
- materials including `widget-alpha-5x`, `widget-alpha-5x-nosharp`,
  `widget-add-5x`, `widget-text-alpha-5x`, and `trsl-colored`;
- the native recipe and item `iconSprite` values;
- Replicator and game-menu sprites such as `panel-1`,
  `crystal-btn-black`, and `circle-extra-thin`; and
- standard sounds `ui-hover-0`, `ui-click-0`, and `ui-click-1`.

Reuse live references from native controls where possible. Asset names are
evidence useful for diagnosis, not a stable string-loading contract.

The Replicator's recipe cells are rendered entries, not cloneable GameObjects.
Tracker rows therefore need tracker-owned runtime UI instances composed with
the same game-provided resources. This satisfies the no-bespoke-assets
principle without pretending an individual native recipe-button prefab exists.

The focused render probe confirmed the Replicator background material is
`storage-bg(Clone)` using shader `UI Ex/Storage Bg`. It exposes a 14-by-8
`_StateBuffer`, `_FilterColor`, and `_BansColor`. In the original Replicator
state, mask `0x1` carries hover and mask `0x8` carries the native red
machine-only/banned treatment; mutating that original array would conflate
tracker state with game state and is rejected.

The feasible treatment is one non-raycasting clone of `recipeBg` with:

- a cloned native material rebound to a separate 120-entry compute buffer;
- tracker state mask `0x2` and `_FilterColor` for unpinned green;
- tracker state mask `0x8` and `_BansColor` for pinned red;
- restrained `Image.color` alpha; and
- sibling placement below recipe icons so the native icon remains legible.

Because the clone has its own buffer, these masks do not overwrite native
hover, selection, disabled, or machine-only state. The calibrated probe
rendered green at approximately `(0.404, 0.647, 0.486, 1)` and red at
approximately `(0.561, 0.208, 0.208, 1)`. Refresh the overlay when pin state
or the populated recipe array changes, and release the cloned object,
material, buffer, and event listener during shutdown.

### 7. Missing and modded recipes

`ProtoSet<T>.Select(int)` checks its index and returns null for an unknown ID.
Tracker state should retain integer recipe IDs, resolve them at refresh, and
fail softly if a recipe, output item, icon, or parallel recipe array is absent
or inconsistent.

The failure policy is to skip or evict the invalid row, preserve the relative
order of remaining pins, and log the identity once rather than throw during UI
refresh. A modded recipe should work when it is registered in LDB and conforms
to the same structural invariants, but that is an informed expectation rather
than a tested compatibility promise.

### 8. Initial compatibility boundary

The initial technical baseline is the exact game, Unity, assembly, BepInEx,
and Harmony versions recorded above. The isolated probe at 1280-by-720
reported an overlay-canvas scale factor of approximately `0.8108108`, but a
main-menu observation does not establish in-game layout correctness.

Supported resolutions, UI scales, and interaction behavior must be selected
and validated during the presentation sprint. Compatibility with
Replicator-altering mods remains evidence-driven; no inspected installed
plugin currently established a conflict.

### 9. Possible future automatic replication

`RecipeProto.Handcraft` is the authoritative eligibility boundary, and the
native Replicator Produce path observes it. If automatic replication is ever
promoted, only hand-craftable recipes may be eligible. Machine-only recipes
remain tracking-only, and the mod must not bypass native forge rules.

Automatic replication is still outside the MVP.

### 10. Possible future persistence

`RecipeProto.ID` is the correct identity inside the fixed runtime snapshot.
It is not guaranteed stable across game updates or arbitrary mod load orders.
If persistence is promoted, store the ID with a lightweight diagnostic
fingerprint, re-resolve through LDB, and discard mismatches safely.

Whether pins belong per save, per profile, or globally remains an owner
decision. BepInEx configuration is global and must not be treated as per-save
storage by inference.

### 11. Global control during major interfaces

The global control should inherit `UIGame.gameMenu` visibility rather than
maintain a separate major-interface rule. The tracker panel independently
uses the exact six-window rule. This answers the earlier design question
without another patch set.

### 12. Localization

The supported assembly exposes `Localization.Translate(string)`,
`Localization.CanTranslate(string)`, and `Localization.OnLanguageChange`.
`RecipeProto.madeFromString` already returns a localized production-category
label, and native controls may carry `Localizer` components.

No suitable public API for registering new translation keys was confirmed.
The safe MVP boundary is to reuse native-localized values and provide a
documented fallback for any tracker-owned text. A cloned control must not keep
an unrelated native localization key. A full translation table remains a
feature-development decision.

## Remaining runtime checkpoints

The feasibility phase did not replace the following in-game validation:

- calibrate the proven green/red layer's opacity and sibling placement over
  live recipe icons and native hover, selection, and machine-only states;
- construct tracker recipe presentation from native resources and verify that
  it reads as an integrated clone rather than a custom control;
- validate initialized layout, dragging, click capture, and the six-window
  hide rule;
- test exact-recipe navigation where multiple recipes share a product;
- select the supported resolution and UI-scale matrix; and
- investigate mod compatibility only when a concrete conflict is observed.
