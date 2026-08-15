# DSP Recipe Tracker

Pin recipes directly from the Replicator and track their immediate ingredient
requirements against the materials in Icarus's inventory.

## Installation

Install with a Thunderstore-compatible mod manager, or extract the package into
the Dyson Sphere Program game directory with BepInEx 5.4.17 installed. The DLL
should end up at:

```text
BepInEx/plugins/DSPRecipeTracker/DSPRecipeTracker.dll
```

## Using the tracker

1. Open the Replicator.
2. Right-click an unlocked recipe to pin it. A green corner treatment marks
   pinned recipe cells; right-click the same recipe again to unpin it.
3. Close the Replicator to view the tracker. Left-click continues to use the
   game's normal recipe-selection behavior and never changes pins.

The newest pin appears at the top. You can track up to three recipes; pinning a
fourth replaces the bottom entry. Crafting does not remove a pin.

## Reading the tracker

- `TARGET` identifies the product being tracked.
- `INGREDIENTS` shows its immediate ingredients in native recipe order.
- Each quantity is `in Icarus inventory / required for one recipe operation`.
- Green quantities are sufficient; red quantities are insufficient.
- Recipes that require a production facility show its localized name beneath
  the row.

Counts update while you play. Only Icarus's inventory is counted—storage,
logistics stations, factories, and materials elsewhere are not included.
Ingredient recipes are not expanded recursively.

## Position and visibility

- Drag the panel to move it within the screen.
- Use the Replicator-icon button on the panel to hide it.
- Restore or toggle it with the matching Replicator-icon button in the
  bottom-right game-menu group, directly above the screenshot button.
- The tracker hides automatically while Tech, Dyson Editor, Inventory,
  Replicator, Statistics, or Dashboard is open, then follows your previous
  manual visibility choice when that interface closes.

Pins, panel position, and visibility choice are session-only and reset when a
game session ends or another save is loaded. Tracker icons are informational;
they do not navigate, craft, move items, or change save data.

Project source and issue reporting:
[github.com/shytamir/DSPRecipeTracker](https://github.com/shytamir/DSPRecipeTracker)
