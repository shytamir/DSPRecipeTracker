# Dyson Sphere Program: Pinned Recipe Tracker

> **Authority notice:** This is the original, non-authoritative concept
> foundation retained for planning context and design history. It does not
> override [`docs/PROJECT.md`](../PROJECT.md), which governs current product
> scope and accepted decisions whenever the documents differ.

## Product concept record

**Status:** Concept ready for feasibility investigation and MVP planning  
**Working title:** Pinned Recipe Tracker  
**Game:** Dyson Sphere Program  
**Source:** Reformatted from a Discord design conversation between fantaflesje and alex2ez. The conversation times span 20:57–01:47; no date was included in the source.  
**Purpose of this record:** Preserve the concept as a reliable basis for a future mod. Confirmed decisions, optional enhancements, and unresolved matters are kept separate.

## 1. Product intent

Provide an unobtrusive in-game panel where the player can pin up to three unlocked recipes and see whether the required ingredients are present in Icarus's inventory.

The panel is intended to reduce repeated trips to the Replicator while the player gathers or automates components. It is an information and navigation aid first; crafting automation is not part of the default experience.

## 2. Core experience

1. The player opens the Replicator and pins an unlocked recipe.
2. The recipe appears in the tracker with its product icon and immediate ingredient icons.
3. Each ingredient shows `amount in inventory / amount required`.
4. Ingredient status changes from red to green when the player has enough.
5. Selecting the product or an ingredient in the tracker opens its recipe in the Replicator, following normal Replicator-style navigation.
6. The tracker remains pinned after crafting. A recipe leaves the tracker only when the player explicitly unpins it or when it is displaced by a newly pinned recipe beyond the three-recipe limit.

## 3. Confirmed requirements

### Recipe selection

- Recipes are pinned from the Replicator.
- Only recipes unlocked by the player's current technology may be pinned.
- All unlocked item recipes are eligible, including buildings.
- Pinning an already pinned recipe unpins it.
- The Replicator should expose a small Pin/Unpin control near the upper-left of the normal Produce control.
- The control should visually match Dyson Sphere Program's existing UI:
  - unpinned: dark and labelled **Pin**;
  - pinned: lit and labelled **Unpin**.
- The tracker holds no more than three recipes.
- Pinning a fourth recipe evicts the current third/bottom entry.

### Material tracking

- Track materials in Icarus's inventory only.
- Do not include storage, logistics stations, or total supplies on the planet.
- Show only the pinned recipe's direct ingredients; do not recursively expand ingredients to raw materials.
- For every ingredient, show `inventory quantity / required quantity`.
- Use red when the inventory amount is insufficient and green when it is sufficient.
- Ingredients use icons without item-name text.

### Recipes requiring a production building

- Keep these recipes pinnable and display their requirements normally.
- Under the pinned product icon, show in small red text which building is required to produce it.
- Do not show production-building labels for the pinned recipe's ingredients. The panel remains focused on the selected recipe rather than earlier production steps.

### Panel presentation

- Default location: left-middle of the screen, below the goals area and above the planet map.
- The player may drag the panel to another location.
- The panel must remain fully reachable within the screen bounds. Either constrain dragging or immediately clamp it back inside the screen.
- Use one static panel size for the initial implementation. A suitable reference is the size of the Goals UI, or slightly larger.
- Use the same general typography as the small Goals text. The conversation suggested approximately 12 pt, bold Arial, but visual matching to the game takes precedence over that estimate.
- The panel should appear above ordinary HUD elements such as the planet map, but not above major opened interfaces such as the Replicator or inventory.
- The panel is interactive and must not pass clicks through to the game world or UI below it.
- The panel automatically disappears when no recipes are pinned.

### Visibility controls

- Provide a Hide control on the panel itself.
- Provide a persistent global Show/Hide button beneath the smallest button in the bottom-left HUD cluster, styled to look native to the game.
- When hidden, the tracker panel disappears and can be restored with the global button.
- The panel automatically hides while the Replicator, inventory, or a comparable major UI is open.
- A collapsed state was requested earlier in the discussion. It may be satisfied by the final Hide/Show design unless later UX testing demonstrates that a distinct collapsed tab is useful.

### Interaction and feedback

- Clicking the pinned recipe icon opens that recipe in the Replicator.
- Clicking an ingredient icon opens that ingredient's recipe in the Replicator, consistent with normal Replicator behavior.
- Pinning may reuse the game's Produce click sound.
- No additional sound design is required.
- No ongoing crafting animation or special text style is required.
- Crafting never removes the pin automatically.

## 4. MVP scope

The first useful release should contain:

- pinning and unpinning from the Replicator;
- a maximum of three pinned recipes;
- unlocked-recipes-only enforcement;
- direct-ingredient display using product and ingredient icons;
- inventory-only quantity tracking;
- red/green sufficiency states;
- required-building label for a pinned recipe that cannot be hand-crafted;
- draggable, screen-bounded panel with a fixed initial size;
- automatic hiding around major UI and when empty;
- panel Hide control and persistent global Show/Hide control;
- navigation from tracker icons to the corresponding Replicator recipe;
- explicit pins that survive crafting for the current play session.

## 5. Desired follow-up features

These were positively received but should not be treated as MVP commitments until feasibility and effort are known.

### Per-recipe target quantity

Allow the player to select how many copies of the pinned product they intend to make. Multiply the required ingredient counts accordingly.

### Saved pins

Persist pinned recipes and restore them when the relevant save is loaded. The requester wants this behavior but left the storage design open.

### Automatic replication

Offer an optional setting that automatically starts hand-crafting when all requirements are available. It must default to **off**. This feature requires validation against recipe craftability and normal game behavior.

### Optional transitions

- Pin: slide the new entry in from the top.
- Unpin: fade the removed entry out.
- Fourth pin: remove the bottom entry, shift remaining entries down, and slide the new entry in.
- Recipe navigation: pulse the selected tracker icon before opening the Replicator.

These are polish features and must not block the static tracker.

## 6. Explicit non-goals for the initial version

- Tracking materials in storage or across the local planet.
- Recursively expanding a recipe to raw materials.
- Pinning locked recipes.
- Automatically unpinning a completed or crafted recipe.
- Displaying the production building for every ingredient.
- Mandatory automatic crafting.
- Resizable panel UI.
- Custom sound packs or elaborate crafting effects.

## 7. Open design and technical questions

These require investigation or owner review before implementation is considered fully specified:

1. **Replicator integration:** Can a readable Pin/Unpin control fit near the Produce control at supported resolutions and UI scales without obscuring native controls?
2. **Major-UI hide rules:** Which exact game windows count as a major UI, and can the mod subscribe to their visibility cleanly instead of maintaining fragile special cases?
3. **Recipe navigation:** Can the Replicator be opened directly to a chosen product or ingredient using stable game APIs or reliable patches?
4. **Craftability:** How should a pinned recipe that requires a machine behave if automatic replication is later enabled?
5. **Persistence boundary:** Should pins be stored per save, per player profile, or globally? What stable identifier should represent a recipe across game and mod updates?
6. **Global button behavior:** Should the bottom-left Show/Hide control remain visible during major UI screens, or hide with the rest of the tracker integration?
7. **Ordering:** The proposed behavior treats the newest pin as the first/top entry and evicts the third/bottom entry. Confirm this ordering during a UI prototype.
8. **Localization:** Decide whether Pin, Unpin, and production-building labels will use DSP's localization system and how unsupported languages fall back.
9. **Compatibility:** Identify conflicts with mods that alter the Replicator or bottom-left HUD cluster.
10. **Layout validation:** Verify the default anchor, static dimensions, font match, icon readability, and screen clamping at common resolutions and UI scales.

## 8. Acceptance outline for a future MVP

A future MVP can be considered technically ready for owner review when all of the following are demonstrated in-game:

- A locked recipe cannot be pinned and an unlocked recipe can.
- Pinning the same recipe twice toggles it off.
- A fourth pin replaces the third/bottom entry exactly as specified.
- Inventory changes update every displayed numerator without reopening the panel.
- Red/green states change at the exact required threshold.
- Only direct ingredients are shown.
- Machine-only recipes name the required building under the product, while their ingredients do not.
- Recipe and ingredient icons navigate to the correct Replicator entries.
- Crafting a pinned recipe does not remove it.
- The panel can be dragged but cannot become unreachable.
- The panel hides and returns correctly around major UI, manual Hide/Show, and an empty pin list.
- The tracker and its controls remain usable at supported resolutions and UI scales.
- No click-through or unintended game-world interaction occurs while using the panel.

Saved pins, quantity multiplication, automatic replication, transitions, and resizing require their own acceptance criteria if promoted into a release scope.

## 9. Related mod ideas retained from the conversation

These ideas were mentioned but were not developed into specifications during this discussion. They should remain separate candidates rather than features of the Pinned Recipe Tracker.

### Automatic miner positioner

- Automatically position Mining Machine Mk.I units to cover every vein in a resource patch.
- Use the mouse wheel to adjust the number of miners.
- Feasibility, placement rules, collision handling, power coverage, and player confirmation are unresolved.

### Automatic geothermal positioner

- On magma planets, automatically place geothermal power stations.
- Possible objective: achieve at least 100% coverage/output, or place as many valid stations as possible.
- The intended optimization target and technical feasibility are unresolved.

## 10. Decision history distilled from the conversation

- Initial request: keep two or three recipes visible on the side of the screen to track what must be made.
- Scope refined to Icarus inventory only, direct ingredients only, and color-coded sufficiency.
- All unlocked item and building recipes became eligible for pinning through the Replicator.
- The panel became draggable, screen-bounded, non-click-through, and hidden around major UI.
- The recipe limit settled at three, with the third/bottom entry displaced by a fourth.
- Product and ingredient icons replaced text-heavy rows; machine-only recipes gained a small required-building label.
- Pins became persistent until explicit unpinning, independent of crafting completion.
- Tracker icons gained Replicator navigation.
- Quantity selection, persistence across saves, automatic replication, and animations remained follow-up work rather than core requirements.
